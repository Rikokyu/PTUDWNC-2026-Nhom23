import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

export function middleware(request: NextRequest) {
  // Sessions currently live in browser storage and are verified by AuthProvider
  // against /auth/me. Never infer permissions from an unverified role cookie.
  if (request.nextUrl.pathname === "/recipes/new") {
    return NextResponse.redirect(new URL("/dashboard/recipes/new", request.url));
  }
  const editRoute = request.nextUrl.pathname.match(/^\/recipes\/([^/]+)\/edit$/);
  if (editRoute) {
    return NextResponse.redirect(new URL(`/dashboard/recipes/${editRoute[1]}/edit`, request.url));
  }
  return NextResponse.next();
}

export const config = {
  matcher: ["/dashboard/:path*", "/recipes/new", "/recipes/:id/edit"],
};
