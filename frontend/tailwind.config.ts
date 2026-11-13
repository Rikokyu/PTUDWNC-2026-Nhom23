import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./src/pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/app/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/features/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        brand: {
          DEFAULT: "#E11D48",
          hover: "#BE123C",
          active: "#9F1239",
          light: "#FFF1F2",
          border: "#FECDD3",
        },
        accent: {
          DEFAULT: "#0F766E",
          hover: "#0D5F59",
          light: "#CCFBF1",
        },
        pastel: {
          purple: "#D8C7FF",
          "purple-light": "#F5F0FF",
        },
        surface: {
          DEFAULT: "#FFFFFF",
          muted: "#F3F4F2",
          subtle: "#E8EAE6",
          cream: "#F7F8F6",
        },
        content: {
          primary: "#0B0B0B",
          secondary: "#3F3F46",
          tertiary: "#71717A",
          disabled: "#A1A1AA",
        },
      },
      fontFamily: {
        sans: ["var(--font-sans)", "Manrope", "system-ui", "sans-serif"],
        display: ["var(--font-display)", "var(--font-sans)", "system-ui", "sans-serif"],
      },
      boxShadow: {
        subtle: "0 1px 2px 0 rgba(11, 11, 11, 0.04)",
        card: "0 1px 0 rgba(11, 11, 11, 0.04), 0 12px 28px -16px rgba(11, 11, 11, 0.18)",
        "card-hover": "0 1px 0 rgba(11, 11, 11, 0.05), 0 24px 40px -20px rgba(11, 11, 11, 0.28)",
        glass: "0 8px 24px -12px rgba(11, 11, 11, 0.16)",
        dropdown: "0 16px 40px -18px rgba(11, 11, 11, 0.28)",
      },
      borderRadius: {
        lg: "0.5rem",
        xl: "0.75rem",
        "2xl": "1rem",
        "3xl": "1.5rem",
      },
      keyframes: {
        "fade-up": {
          "0%": { opacity: "0", transform: "translateY(18px)" },
          "100%": { opacity: "1", transform: "translateY(0)" },
        },
        "hero-zoom": {
          "0%": { transform: "scale(1.08)" },
          "100%": { transform: "scale(1)" },
        },
        "soft-pulse": {
          "0%, 100%": { opacity: "0.55" },
          "50%": { opacity: "0.85" },
        },
      },
      animation: {
        "fade-up": "fade-up 0.7s ease-out both",
        "hero-zoom": "hero-zoom 8s ease-out both",
        "soft-pulse": "soft-pulse 3.5s ease-in-out infinite",
      },
    },
  },
  plugins: [],
};

export default config;
