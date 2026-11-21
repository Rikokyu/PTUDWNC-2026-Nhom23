import { proxyGeneratedFile } from "@/lib/sitemap/proxy";

export const dynamic = "force-dynamic";

export function GET() {
  return proxyGeneratedFile("sitemap.xml");
}
