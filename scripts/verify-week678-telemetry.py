"""Capture collector evidence without printing configured secrets."""
import json
import os
from pathlib import Path
import re
import subprocess

root = Path(__file__).resolve().parents[1]
result = subprocess.run(['docker', 'compose', '-f', 'docker-compose.yml', '-f', 'docker-compose.observability.yml',
                         'logs', '--no-color', '--since', '30m', 'otel-collector'], cwd=root, capture_output=True, text=True, encoding='utf-8')
text = result.stdout
(root / 'week678-collector.log').write_text(text + result.stderr, encoding='utf-8')
names = ['culinary.recipe.searches', 'culinary.recipe.searches.empty', 'culinary.recipe.changes',
         'culinary.image.uploads', 'culinary.operation.duration']
summary = {'collectorExitCode': result.returncode,
           'traceIds': len(re.findall(r'Trace ID\s+: [0-9a-f]{32}', text)),
           'spanIds': len(re.findall(r'\|\s+ID\s+: [0-9a-f]{16}', text)),
           'npgsqlOccurrences': text.count('Npgsql'),
           'minioUploadSpans': len(re.findall(r'Name\s+: minio.upload', text)),
           'propagatedTraceFound': '0123456789abcdef0123456789abcdef' in text,
           'metrics': {name: len(re.findall(re.escape('Name: ' + name) + r'\s', text)) for name in names}}
settings = json.loads((Path(os.environ['APPDATA']) / 'Microsoft/UserSecrets/CulinaryBlog-API-Development/secrets.json').read_text(encoding='utf-8-sig'))
api_log = (root / 'week678-runtime-api.log').read_text(encoding='utf-8')
collector_ids = set(re.findall(r'Trace ID\s+: ([0-9a-f]{32})', text))
api_ids = set(re.findall(r'"TraceId":"([0-9a-f]{32})"', api_log))
summary['traceIdsSharedWithApiLogs'] = len(collector_ids & api_ids)
summary['w3cTraceInApiLogs'] = '0123456789abcdef0123456789abcdef' in api_log
summary['knownSecretsInCollector'] = sum(1 for key, value in settings.items()
                                       if any(part in key for part in ['Secret', 'AccessKey']) and value in text)
summary['jwtPatternsInCollector'] = len(re.findall(r'eyJ[A-Za-z0-9_-]+\.eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+', text))
summary['businessMetricSamples'] = {}
for name in names:
    starts = [match.start() for match in re.finditer(re.escape('Name: ' + name) + r'\s', text)]
    if starts:
        start = starts[-1]
        end = text.find('Metric #', start)
        block = text[start:end if end != -1 else start + 4000]
        summary['businessMetricSamples'][name] = {
            'values': re.findall(r'\| Value: ([^\r\n]+)', block)[:8],
            'counts': re.findall(r'\| Count: ([^\r\n]+)', block)[:8]}
(root / 'docs/verification/WEEK678_RUNTIME_EVIDENCE.json').write_text(json.dumps(summary, indent=2), encoding='utf-8')
print(json.dumps(summary, indent=2))
assert result.returncode == 0 and summary['traceIds'] > 0 and summary['spanIds'] > 0
assert summary['npgsqlOccurrences'] > 0 and summary['minioUploadSpans'] > 0
assert summary['w3cTraceInApiLogs'] and summary['traceIdsSharedWithApiLogs'] > 0 and all(summary['metrics'].values())
assert summary['knownSecretsInCollector'] == 0 and summary['jwtPatternsInCollector'] == 0
