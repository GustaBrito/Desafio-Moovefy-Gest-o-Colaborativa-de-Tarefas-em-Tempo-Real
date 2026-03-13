import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  test: {
    environment: "jsdom",
    globals: true,
    setupFiles: "./src/testes/configurarTestes.ts",
    css: true,
    pool: "forks",
    maxWorkers: 1,
    fileParallelism: false,
    isolate: false,
    execArgv: ["--max-old-space-size=8192"],
    coverage: {
      provider: "v8",
      reporter: ["text", "html", "lcov"],
      include: ["src/**/*.{ts,tsx}"],
      exclude: ["src/main.tsx", "src/vite-env.d.ts"],
    },
  },
});
