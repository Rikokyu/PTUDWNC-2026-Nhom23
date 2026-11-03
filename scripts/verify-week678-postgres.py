"""Run existing integration tests against uniquely named test databases.

Reads the existing development connection without printing it. Test databases
are retained; this command never resets or deletes the application database.
"""
import json
import os
from pathlib import Path
import subprocess
import sys

root = Path(__file__).resolve().parents[1]
secrets = Path(os.environ["APPDATA"]) / "Microsoft/UserSecrets/CulinaryBlog-API-Development/secrets.json"
connection = os.environ.get("CULINARYBLOG_TEST_POSTGRES")
if not connection and secrets.exists():
    settings = json.loads(secrets.read_text(encoding="utf-8-sig"))
    connection = settings.get("ConnectionStrings:DefaultConnection") or settings.get("ConnectionStrings", {}).get("DefaultConnection")
if not connection:
    print("UNVERIFIED: no existing development PostgreSQL connection available.")
    sys.exit(2)
environment = os.environ.copy()
environment["CULINARYBLOG_TEST_POSTGRES"] = connection
environment["CULINARYBLOG_KEEP_TEST_DATABASES"] = "1"
with (root / "week678-postgres-tests.log").open("w", encoding="utf-8") as log:
    result = subprocess.run([
        "dotnet", "test", str(root / "backend/CulinaryBlog.slnx"), "--no-build", "--no-restore",
        "--filter", "FullyQualifiedName~RecipeFullTextSearchTests|FullyQualifiedName~MissingFeaturesHttpTests",
        "--logger", "trx;LogFilePrefix=week678-postgres",
    ], cwd=root, env=environment, stdout=log, stderr=subprocess.STDOUT)
print(f"PostgreSQL integration test exit code: {result.returncode}. See week678-postgres-tests.log.")
sys.exit(result.returncode)
