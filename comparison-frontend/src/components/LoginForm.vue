<template>
  <div class="login-container">
    <el-card class="login-card">
      <template #header>
        <h2 class="card-title">Excel文件比对系统</h2>
      </template>
      <el-form :model="loginForm" :rules="rules" ref="loginFormRef">
        <el-form-item prop="username">
          <el-input 
            v-model="loginForm.username" 
            placeholder="用户名"
            prefix-icon="User"
          />
        </el-form-item>
        <el-form-item prop="password">
          <el-input 
            v-model="loginForm.password" 
            type="password" 
            placeholder="密码"
            prefix-icon="Lock"
            show-password
          />
        </el-form-item>
        <el-form-item>
          <el-button 
            type="primary" 
            @click="handleLogin" 
            :loading="loading"
            style="width: 100%"
          >
            登录
          </el-button>
        </el-form-item>
      </el-form>
      <div class="login-tip">
        默认用户名：admin<br>
        默认密码：admin
      </div>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'

const router = useRouter()
const loading = ref(false)
const loginFormRef = ref(null)

// 设置固定的用户名和密码
const VALID_USERNAME = 'admin'
const VALID_PASSWORD = 'admin'

const loginForm = reactive({
  username: '',
  password: ''
})

const rules = {
  username: [
    { required: true, message: '请输入用户名', trigger: 'blur' }
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' }
  ]
}

const handleLogin = async () => {
  if (!loginFormRef.value) return
  
  await loginFormRef.value.validate((valid) => {
    if (!valid) return
    
    loading.value = true
    
    // 验证用户名和密码
    if (loginForm.username === VALID_USERNAME && loginForm.password === VALID_PASSWORD) {
      ElMessage.success('登录成功')
      router.push('/comparison')
    } else {
      ElMessage.error('用户名或密码错误')
    }
    
    loading.value = false
  })
}
</script>

<style scoped>
.login-container {
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background: linear-gradient(to bottom right, #f0f4f8, #ffffff);
}

.login-card {
  width: 400px;
  border-radius: 8px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.card-title {
  text-align: center;
  margin: 0;
  color: #2c3e50;
}

.login-tip {
  text-align: center;
  color: #909399;
  font-size: 14px;
  margin-top: 10px;
}
</style> 