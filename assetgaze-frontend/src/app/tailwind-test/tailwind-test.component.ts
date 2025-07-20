// assetgaze-frontend/src/app/tailwind-test/tailwind-test.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common'; // Needed for common directives if used

@Component({
  selector: 'app-tailwind-test',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-8 bg-gradient-to-br from-blue-500 to-purple-600 text-white min-h-screen flex flex-col items-center justify-center space-y-6">
      <h1 class="text-5xl font-extrabold tracking-tight text-center drop-shadow-lg sm:text-6xl lg:text-7xl">
        <span class="block">Tailwind CSS v4.0 Test</span>
      </h1>
      <p class="mt-4 text-xl text-center max-w-2xl">
        If you see this vibrant styling, Tailwind CSS v4.0 is working!
      </p>
      <div class="flex flex-wrap justify-center gap-4 mt-8">
        <button class="px-6 py-3 bg-white text-blue-700 font-semibold rounded-full shadow-lg hover:shadow-xl transform hover:scale-105 transition duration-300 ease-in-out">
          Test Button 1
        </button>
        <button class="px-6 py-3 bg-yellow-300 text-gray-800 font-semibold rounded-full shadow-lg hover:shadow-xl transform hover:scale-105 transition duration-300 ease-in-out">
          Test Button 2
        </button>
      </div>
      <div class="mt-12 w-full max-w-sm bg-gray-800 p-6 rounded-lg shadow-2xl border-4 border-yellow-400">
        <p class="text-lg text-center font-mono text-green-400">
          This box uses specific Tailwind classes.
        </p>
      </div>
    </div>
  `,
  styles: [] // No specific styles, all from Tailwind
})
export class TailwindTestComponent {}
