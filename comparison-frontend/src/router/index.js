import { createRouter, createWebHistory } from 'vue-router'
import LoginForm from '../components/LoginForm.vue'
import ComparisonForm from '../components/ComparisonForm.vue'
<<<<<<< HEAD
import axios from 'axios'
import { API_URL } from '@/config/api'
=======
>>>>>>> 01_branch

const routes = [
  {
    path: '/',
    redirect: '/login'
  },
  {
    path: '/login',
<<<<<<< HEAD
=======
    name: 'Login',
>>>>>>> 01_branch
    component: LoginForm
  },
  {
    path: '/comparison',
<<<<<<< HEAD
=======
    name: 'Comparison',
>>>>>>> 01_branch
    component: ComparisonForm,
    meta: { requiresAuth: true }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// 路由守卫
<<<<<<< HEAD
router.beforeEach(async (to, from, next) => {
  if (to.matched.some(record => record.meta.requiresAuth)) {
    try {
      const response = await axios.get(`${API_URL}/api/users/check`, {
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
=======
router.beforeEach((to, from, next) => {
  // 这里可以添加登录验证逻辑
  // 暂时简单处理，后续可以根据实际需求完善
  next()
>>>>>>> 01_branch
})

export default router 