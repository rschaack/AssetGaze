/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}", // This line is essential
  ],
  theme: {
    extend: {
      // You can leave this empty if you define all your
      // custom styles in styles.css, but the theme key must exist.
    },
  },
  plugins: [],
};
