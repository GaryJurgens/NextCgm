# Active Context

Currently focused on standardizing the global Vue.js state management of the `Portal` project to ensure that session and authorization data reliably persist across pages and reloads.

## Recent Changes
- Included `pinia` and `pinia-plugin-persistedstate` into `Portal/package.json`.
- Implemented `useAuthStore` in `Portal/src/stores/auth.js` to manage session token, user details, and OTP email state.
- Updated `main.js` and `router/index.js` to hook the Pinia store with route guard middleware ensuring redirection logic references the centralized auth.
- Refactored Views (`Login`, `Register`, `VerifyOtp`, `Dashboard`, `Settings`, `Payment`) to rely on Pinia action mutations and state getters rather than manual `localStorage` invocations.
- Allowed environment switching via `import.meta.env.VITE_API_BASE_URL` in the `services/api.js` base URL setup.
- Resolved an export naming mismatch in `Settings.vue` accessing `getUserId` rather than `getUserEntityId`.

## Next Steps
- Implement HIPAA and GDPR policies correctly.
- Review additional components or API functions for any legacy `localStorage` remnants that can migrate to global Pinia state.
- Test app navigation flows and session expiry checks locally.