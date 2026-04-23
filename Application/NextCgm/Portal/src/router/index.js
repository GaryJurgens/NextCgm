import { createRouter, createWebHistory } from 'vue-router';
import Login from '../views/Login.vue';
import Register from '../views/Register.vue';
import Dashboard from '../views/Dashboard.vue';
import VerifyOtp from '../views/VerifyOtp.vue';
import Payment from '../views/Payment.vue';
import Settings from '../views/Settings.vue';
import PaymentVerify from '../views/PaymentVerify.vue';
import { useAuthStore } from '../stores/auth';

const routes = [
  { 
    path: '/', 
    redirect: to => {
      // If we land on root with payment reference query params from an external gateway,
      // redirect to the payment verify page instead of login.
      if (to.query.reference || to.query.trxref || to.query.transaction_id) {
        return { path: '/payment-verify', query: to.query };
      }
      return '/login';
    } 
  },
  { path: '/login', name: 'Login', component: Login, meta: { isPublic: true } },
  { path: '/register', name: 'Register', component: Register, meta: { isPublic: true } },
  { path: '/verify-otp', name: 'VerifyOtp', component: VerifyOtp, meta: { isPublic: true } },
  { path: '/dashboard', name: 'Dashboard', component: Dashboard, meta: { requiresAuth: true } },
  { path: '/payment', name: 'Payment', component: Payment, meta: { requiresAuth: true } },
  { 
    path: '/payment-verify', 
    name: 'PaymentVerify', 
    component: PaymentVerify,
    // Sensitive to the external gateway's behavior
    alias: ['/payment-verify/', '/PaymentVerify', '/paymentverify', '/verify', '/verfy'],
    meta: { isPublic: true } 
  },
  { path: '/settings', name: 'Settings', component: Settings, meta: { requiresAuth: true } },
  // TEMPORARILY: Remove the redirect for unknown paths to see if you get a blank page instead of login
  { path: '/:pathMatch(.*)*', component: { template: '<div>Route Not Found</div>' } }
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  
  if (!authStore.token && localStorage.getItem('token')) {
    authStore.initializeAuth();
  }

  // LOGGING: Add this temporarily to your console to see what the router thinks is happening
  console.log('Navigating to:', to.path, 'Is Public:', to.meta.isPublic);

  const pathLower = to.path.toLowerCase();
  // If the path contains our keyword, force it to be public regardless of metadata
  if (pathLower.includes('payment-verify') || pathLower.includes('/verify') || pathLower.includes('/verfy')) {
    return next();
  }

  if (to.meta.isPublic) {
    return next();
  }

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return next({ path: '/login', query: { redirect: to.fullPath } });
  }

  next();
});

export default router;