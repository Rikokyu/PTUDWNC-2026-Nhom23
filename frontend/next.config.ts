import type { NextConfig } from "next";
import { PHASE_DEVELOPMENT_SERVER } from "next/constants";

const nextConfig: NextConfig = {
  async rewrites() {
    return [
      { source: "/uploads/:path*", destination: `${process.env.API_BASE_URL ?? "http://127.0.0.1:5080"}/uploads/:path*` },
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
