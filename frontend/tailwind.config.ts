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
          DEFAULT: "#A94F2D",
          hover: "#873D24",
          active: "#6D321E",
          light: "#F8EDE5",
          border: "#E8C9B5",
        },
        accent: {
          DEFAULT: "#586A42",
          hover: "#435334",
          light: "#EDF1E6",
        },
        surface: {
          DEFAULT: "#FFFEFB",
          muted: "#F5F2EB",
          subtle: "#EAE4D8",
          cream: "#FBF8F1",
        },
        content: {
          primary: "#29271F",
          secondary: "#514F45",
          tertiary: "#777368",
          disabled: "#9F9B91",
        },
      },
      fontFamily: {
        sans: ["var(--font-sans)", "Manrope", "system-ui", "sans-serif"],
        display: ["var(--font-display)", "var(--font-sans)", "system-ui", "sans-serif"],
      },
      boxShadow: {
        subtle: "0 2px 8px -5px rgba(48, 39, 26, 0.15)",
        card: "0 8px 30px -18px rgba(48, 39, 26, 0.2)",
        "card-hover": "0 22px 46px -22px rgba(48, 39, 26, 0.28)",
        glass: "0 10px 28px -18px rgba(48, 39, 26, 0.22)",
        dropdown: "0 20px 44px -20px rgba(48, 39, 26, 0.25)",
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
