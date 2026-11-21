export async function proxyGeneratedFile(fileName: "sitemap.xml" | "robots.txt"): Promise<Response> {
  const apiBase = process.env.API_BASE_URL ?? "http://127.0.0.1:5080";

  try {
    const upstream = await fetch(new URL(`/${fileName}`, apiBase), { cache: "no-store" });
    if (!upstream.ok) {
      return new Response("Generated file is unavailable.", {
        status: upstream.status,
        headers: { "Cache-Control": "no-store" },
      });
    }

    return new Response(upstream.body, {
      status: 200,
      headers: {
        "Content-Type": fileName === "sitemap.xml"
          ? "application/xml; charset=utf-8"
          : "text/plain; charset=utf-8",
        "Cache-Control": "no-store, max-age=0",
      },
    });
  } catch {
    return new Response("Sitemap service is unavailable.", {
      status: 502,
      headers: { "Cache-Control": "no-store" },
    });
  }
}
