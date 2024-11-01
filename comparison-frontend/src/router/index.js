import { createRouter, createWebHistory } from 'vue-router'
import LoginForm from '../components/LoginForm.vue'
import ComparisonForm from '../components/ComparisonForm.vue'
import axios from 'axios'

const routes = [
  {
    path: '/',
    redirect: '/login'
  },
  {
    path: '/login',
    component: LoginForm
  },
  {
    path: '/comparison',
    component: ComparisonForm,
    meta: { requiresAuth: true }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// 路由守卫
router.beforeEach(async (to, from, next) => {
  if (to.matched.some(record => record.meta.requiresAuth)) {
    try {
      // 修改为正确的检查接口地址
      const response = await axios.get('http://localhost:5000/api/users/check', {
        withCredentials: true
      })
      if (response.data.authenticated) {
        next()
      } else {
        next('/login')
      }
    } catch (error) {
      console.error('认证检查失败:', error)
      next('/login')
    }
  } else {
    next()
  }
})

export default router 