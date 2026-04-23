# System Patterns

## Architecture
- Backend: C# / FastEndpoints.
- Frontend: Vue.js 3 via Vite.

## State Management
- Vue 3 state management is conducted centrally via Pinia (`Portal/src/stores/auth.js`) using `pinia-plugin-persistedstate` to safely replicate user session state to the browser's `localStorage`.

## Routing Guards
- Vue Router relies on Pinia `authStore.isAuthenticated` and global `authStore.token` values internally maintained.
- Navigation guards prevent unauthorized users from viewing specific components like `Dashboard`, `Settings`, and `Payment`.

## API Calls
- Axios instances maintain a dynamic baseURL resolving from `.env` `VITE_API_BASE_URL` enabling instant swaps between dev, test, and production infrastructures.
- Request interceptors inject the `Authorization: Bearer <token>` Header seamlessly when users are authenticated.