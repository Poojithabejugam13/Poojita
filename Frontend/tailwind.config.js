/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        'primary-plum': '#4B2E83',        // Deep Purple
        'secondary-maroon': '#382262',    // Slightly darker Deep Purple for gradients & hovers
        'text-charcoal': '#201236',       // Very dark Deep Purple for highly readable text
        'accent-nude': '#C0C0C0',         // Silver
        'accent-peach': '#A3A3A3',        // Slightly darker Silver for hovers
        'bg-blush': '#F2F2F2',            // Light Grey
        'state-success': '#4A7C59',
        'state-warning': '#E5A93E',
        'state-error': '#A04550',
      },
      fontFamily: {
        sans: ['Inter', 'sans-serif', 'system-ui'],
      },
    },
  },
  plugins: [],
}
