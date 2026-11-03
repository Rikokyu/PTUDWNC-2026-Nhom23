"""Start API against a fresh retained test database and bucket, reusing credentials."""
import json
import os
from pathlib import Path
import re
import socket
import subprocess
import uuid

root = Path(__file__).resolve().parents[1]
with socket.socket() as probe:
    if probe.connect_ex(('127.0.0.1', 5080)) == 0:
        raise SystemExit('Port 5080 is already in use. Refusing to create another verification API.')
settings = json.loads((Path(os.environ['APPDATA']) / 'Microsoft/UserSecrets/CulinaryBlog-API-Development/secrets.json').read_text(encoding='utf-8-sig'))
database = 'culinary_lab8_verify_' + uuid.uuid4().hex[:12]
bucket = 'lab8-verify-' + uuid.uuid4().hex[:12]
info = json.loads(subprocess.check_output(['docker', 'inspect', 'culinary-blog-api-test-postgres'], text=True))[0]
pg_env = dict(entry.split('=', 1) for entry in info['Config']['Env'])
subprocess.run(['docker', 'exec', 'culinary-blog-api-test-postgres', 'psql', '-U', pg_env['POSTGRES_USER'], '-d', 'postgres', '-v', 'ON_ERROR_STOP=1', '-c', f'CREATE DATABASE "{database}"'], check=True, stdout=subprocess.DEVNULL)
connection = re.sub(r'(?i)(Database|Initial Catalog)=[^;]*', 'Database=' + database, settings['ConnectionStrings:DefaultConnection'])
assert database in connection
env = os.environ.copy()
for key, value in settings.items():
    env[key.replace(':', '__')] = str(value)
env.update({
    'ASPNETCORE_ENVIRONMENT': 'Development', 'ASPNETCORE_URLS': 'http://127.0.0.1:5080',
    'ConnectionStrings__DefaultConnection': connection, 'ConnectionStrings__Redis': '127.0.0.1:6380',
    'Hangfire__Enabled': 'false', 'FileStorage__Provider': 'Minio',
    'Minio__Endpoint': 'localhost:9000', 'Minio__Bucket': bucket,
    'Minio__PublicBaseUrl': f'http://localhost:9000/{bucket}', 'Minio__PublicRead': 'true',
    'OpenTelemetry__Enabled': 'true', 'OpenTelemetry__ExportEnabled': 'true',
    'OpenTelemetry__Endpoint': 'http://localhost:4317', 'OTEL_METRIC_EXPORT_INTERVAL': '5000',
    'Smtp__Host': 'localhost', 'Smtp__Port': '1025', 'Smtp__UseSsl': 'false',
    'WelcomeEmail__ApplicationUrl': 'http://localhost:3100', 'App__FrontendBaseUrl': 'http://localhost:3100',
    'Logging__LogLevel__Default': 'Information', 'Serilog__MinimumLevel__Default': 'Information',
})
log = (root / 'week678-runtime-api.log').open('w', encoding='utf-8')
process = subprocess.Popen(['dotnet', 'run', '--no-build', '--no-launch-profile', '--project', str(root / 'backend/src/CulinaryBlog.API')],
    cwd=root, env=env, stdout=log, stderr=subprocess.STDOUT, creationflags=subprocess.CREATE_NO_WINDOW)
state = {'apiPid': process.pid, 'database': database, 'bucket': bucket, 'apiUrl': 'http://127.0.0.1:5080'}
(root / 'week678-runtime-state.json').write_text(json.dumps(state, indent=2), encoding='utf-8')
print(json.dumps(state))
