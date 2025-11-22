/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      screens: {
        '3xl': '1920px', // Large TVs (55"+)
        '4k': '3840px',  // 4K displays - readable from 3m away
      },
    },
  },
  plugins: [],
}