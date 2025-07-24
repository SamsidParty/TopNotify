import js from "@eslint/js";
import eslintPluginReact from "eslint-plugin-react";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import { defineConfig, globalIgnores } from "eslint/config";
import globals from "globals";

export default defineConfig([
    globalIgnores(["dist"]),
    {
        files: ["**/*.{js,jsx}"],
        extends: [
            js.configs.recommended,
            reactHooks.configs["recommended-latest"],
            reactRefresh.configs.vite,
        ],
        languageOptions: {
            ecmaVersion: 2020,
            globals: {
                ...globals.browser,
                igniteView: "readonly",
            },
            parserOptions: {
                ecmaVersion: "latest",
                ecmaFeatures: { jsx: true },
                sourceType: "module"
            },
        },
        plugins: {
            react: eslintPluginReact,
        },
        rules: {
            "no-unused-vars": ["off", { varsIgnorePattern: "^[A-Z_]" }],
            "indent": ["error", 4, { SwitchCase: 1 }],
            "quotes": ["error", "double", { avoidEscape: true }],
            "semi": ["error", "always"],
            "no-var": "error",
            "react-hooks/exhaustive-deps": "off",
            "react/jsx-max-props-per-line": ["error", {
                maximum: 4,
                when: "always",
            }],
            "function-paren-newline": ["error", "multiline-arguments"],
        },
    },
]);