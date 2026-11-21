import NextAuth from "next-auth";
import Google from "next-auth/providers/google";

const clientId = process.env.GOOGLE_CLIENT_ID;
const clientSecret = process.env.GOOGLE_CLIENT_SECRET;

export const googleLoginEnabled = Boolean(
  clientId?.trim() && clientSecret?.trim()
  && !clientId.includes("CHANGE_ME") && !clientSecret.includes("CHANGE_ME"),
);

export const { handlers, auth, signIn, signOut } = NextAuth({
  basePath: "/oauth",
  session: { strategy: "jwt", maxAge: 5 * 60 },
  pages: { signIn: "/auth/login", error: "/auth/login" },
  providers: googleLoginEnabled
    ? [Google({
        clientId: clientId!,
        clientSecret: clientSecret!,
        authorization: { params: { scope: "openid email profile" } },
      })]
    : [],
  callbacks: {
    async jwt({ token, account }) {
      if (account?.provider === "google" && account.id_token) {
        token.googleIdToken = account.id_token;
      }
      return token;
    },
    async session({ session, token }) {
      return { ...session, googleIdToken: token.googleIdToken };
    },
  },
});
