import type { NextConfig } from "next";
import { PHASE_DEVELOPMENT_SERVER } from "next/constants";

const nextConfig: NextConfig = {
  experimental: {
    useWasmBinary: true,
    // Match the avatar upload client's 5-minute timeout instead of Next.js'
    // 30-second rewrite proxy default.
    proxyTimeout: 5 * 60_000,
    // Uploads are proxied through Next.js rewrites. Allow the avatar endpoint's
    // 100 MiB file limit plus multipart framing instead of the 10 MiB default.
    middlewareClientMaxBodySize: "110mb",
  },
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${process.env.API_BASE_URL ?? "http://127.0.0.1:5080"}/api/:path*`,
      },
    ];
  },
  images: {
    remotePatterns: [
      { protocol: "http", hostname: "localhost", port: "5000" },
      { protocol: "http", hostname: "localhost", port: "5080" },
      { protocol: "https", hostname: "lh3.googleusercontent.com" },
    ],
  },
};

export default function config(phase: string): NextConfig {
  return {
    ...nextConfig,
    distDir: phase === PHASE_DEVELOPMENT_SERVER ? ".next" : ".next-production",
  };
}
