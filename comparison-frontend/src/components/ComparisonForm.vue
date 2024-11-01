<template>
    <div class="comparison-container">
      <el-card class="form-card">
        <template #header>
          <h2 class="card-title">Excel文件比对系统</h2>
        </template>
        <el-form :model="form" label-position="top">
          <el-row :gutter="20">
            <el-col :span="12">
              <el-form-item label="标准文件">
                <el-upload
                  class="upload-demo"
                  action="#"
                  :on-change="(file) => handleFileChange(file, 'fileA')"
                  :auto-upload="false"
                  :show-file-list="true"
                >
                  <el-button type="primary" :icon="Upload" class="upload-button">
                    选择标准文件
                    <el-icon class="el-icon--right"><Upload /></el-icon>
                  </el-button>
                </el-upload>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="待比对文件">
                <el-upload
                  class="upload-demo"
                  action="#"
                  :on-change="(file) => handleFileChange(file, 'fileB')"
                  :auto-upload="false"
                  :show-file-list="true"
                >
                  <el-button type="primary" :icon="Upload" class="upload-button">
                    选择待比对文件
                    <el-icon class="el-icon--right"><Upload /></el-icon>
                  </el-button>
                </el-upload>
              </el-form-item>
            </el-col>
          </el-row>
  
          <el-row :gutter="20">
            <el-col :span="12">
              <el-form-item label="长度容差 (%)">
                <el-input-number
                  v-model="form.lengthTolerance"
                  :min="0"
                  :max="100"
                  :precision="2"
                  :step="0.1"
                  style="width: 100%"
                >
                  <template #prefix>
                    <el-icon><Position /></el-icon>
                  </template>
                </el-input-number>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="宽度容差 (%)">
                <el-input-number
                  v-model="form.widthTolerance"
                  :min="0"
                  :max="100"
                  :precision="2"
                  :step="0.1"
                  style="width: 100%"
                >
                  <template #prefix>
                    <el-icon><ScaleToOriginal /></el-icon>
                  </template>
                </el-input-number>
              </el-form-item>
            </el-col>
          </el-row>
  
          <el-form-item label="保存路径">
            <el-input
              v-model="form.savePath"
              placeholder="请选择保存路径"
              readonly
            >
              <template #prefix>
                <el-icon><FolderOpened /></el-icon>
              </template>
              <template #append>
                <el-button @click="handleSelectPath" :icon="Folder">
                  选择路径
                </el-button>
              </template>
            </el-input>
          </el-form-item>
  
          <el-form-item>
            <el-button type="primary" @click="handleCompare" :loading="loading" :icon="VideoPlay" class="action-button">
              开始比对
            </el-button>
            <el-date-picker
              v-model="dateRange"
              type="datetimerange"
              range-separator="至"
              start-placeholder="开始时间"
              end-placeholder="结束时间"
              format="YYYY-MM-DD HH:mm:ss"
              value-format="YYYY-MM-DD HH:mm:ss"
              :default-time="[
                new Date(2000, 1, 1, 0, 0, 0),
                new Date(2000, 1, 1, 23, 59, 59),
              ]"
            />
            <el-button type="info" @click="loadResults" :icon="List" class="action-button">
              查看历史记录
            </el-button>
          </el-form-item>
        </el-form>
      </el-card>
  
      <el-card v-if="results.length > 0" class="result-card">
        <template #header>
          <h3 class="card-title">比对结果</h3>
        </template>
        <el-table 
          :data="results.slice((currentPage-1)*pageSize, currentPage*pageSize)" 
          style="width: 100%" 
          border 
          stripe>
          <el-table-column prop="id" label="编号" />
          <el-table-column prop="length" label="长度" />
          <el-table-column prop="width" label="宽度" />
          <el-table-column prop="status" label="合格否">
            <template #default="scope">
              <el-tag :type="scope.row.status === '合格' ? 'success' : 'danger'">
                {{ scope.row.status }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="exceeded" label="超出范围" />
          <el-table-column prop="fileName" label="比对文件" />
          <el-table-column prop="entryTime" label="录入时间">
            <template #default="scope">
              {{ formatDate(scope.row.entryTime) }}
            </template>
          </el-table-column>
        </el-table>
      </el-card>
  
      <el-pagination
        v-if="results.length > 0"
        :current-page="currentPage"
        :page-size="pageSize"
        :total="results.length"
        layout="total, prev, pager, next"
        @current-change="handlePageChange"
      />
    </div>
  </template>
  
  <script setup>
  import { ref, reactive } from 'vue'
  import { ElMessage } from 'element-plus'
  import { 
    Upload,
    Folder,
    VideoPlay,
    List,
    Position,
    ScaleToOriginal,
    FolderOpened 
  } from '@element-plus/icons-vue'
  import axios from 'axios'
  
  const API_BASE_URL = 'http://localhost:5000/api'
  
  const form = reactive({
    fileA: null,
    fileB: null,
    lengthTolerance: 5,
    widthTolerance: 5,
    savePath: ''
  })
  
  const results = ref([])
  const loading = ref(false)
  const currentPage = ref(1)
  const pageSize = ref(10)
  const dateRange = ref([])
  
  const handleFileChange = (file, type) => {
    form[type] = file.raw
  }
  
  const handleSelectPath = async () => {
    try {
      const response = await axios.get(`${API_BASE_URL}/comparison/selectfolder`, {
        withCredentials: true
      })
      if (response.data && response.data.path) {
        form.savePath = response.data.path
      }
    } catch (error) {
      ElMessage.error('选择路径失败: ' + (error.response?.data || error.message))
    }
  }
  
  const handleCompare = async () => {
    if (!form.fileA || !form.fileB) {
      ElMessage.warning('请选择需要比对的文件')
      return
    }
  
    if (!form.savePath) {
      ElMessage.warning('请输入保存路径')
      return
    }
  
    if (!/^[a-zA-Z]:\\.*$/.test(form.savePath)) {
      ElMessage.warning('请输入正确的Windows路径格式，例：D:\\ExcelResults')
      return
    }
  
    loading.value = true
    try {
      const formData = new FormData()
      formData.append('fileA', form.fileA)
      formData.append('fileB', form.fileB)
      formData.append('lengthTolerance', form.lengthTolerance)
      formData.append('widthTolerance', form.widthTolerance)
      formData.append('savePath', form.savePath)
  
      const response = await axios.post(
        `${API_BASE_URL}/comparison/compare`,
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data',
            'Access-Control-Allow-Origin': '*'
          },
          withCredentials: true
        }
      )
  
      results.value = response.data
      ElMessage.success(`比对完成，文件已保存到：${form.savePath}`)
    } catch (error) {
      // 直接显示后端返回的错误信息
      ElMessage.error(error.response?.data || '比对失败，请稍后重试');
    } finally {
      loading.value = false
    }
  }
  
  const loadResults = async () => {
    if (!dateRange.value || dateRange.value.length !== 2) {
        ElMessage.warning('请选择时间区间')
        return
    }

    try {
        console.log('发送的时间范围:', {
            startTime: dateRange.value[0],
            endTime: dateRange.value[1]
        })

        const response = await axios.get(`${API_BASE_URL}/comparison/results`, {
            params: {
                startTime: new Date(dateRange.value[0]).toISOString(),
                endTime: new Date(dateRange.value[1]).toISOString()
            },
            withCredentials: true
        })

        console.log('接收到的数据:', response.data)
        
        if (response.data && Array.isArray(response.data)) {
            results.value = response.data.map(item => ({
                ...item,
                entryTime: new Date(item.entryTime).toLocaleString()
            }))
        } else {
            ElMessage.warning('未获取到数据')
        }
        currentPage.value = 1
    } catch (error) {
        console.error('加载历史记录错误:', error)
        ElMessage.error('加载历史记录失败: ' + (error.response?.data || error.message))
    }
  }
  
  const formatDate = (date) => {
    return new Date(date).toLocaleString()
  }
  
  const handlePageChange = (val) => {
    currentPage.value = val
  }
  </script>
  
  <style scoped>
  .comparison-container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
    background: linear-gradient(to bottom right, #f0f4f8, #ffffff);
    min-height: 100vh;
  }
  
  .form-card, .result-card {
    border-radius: 8px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1), 0 1px 3px rgba(0, 0, 0, 0.08);
    transition: all 0.3s ease;
  }
  
  .form-card:hover, .result-card:hover {
    box-shadow: 0 6px 8px rgba(0, 0, 0, 0.15), 0 1px 3px rgba(0, 0, 0, 0.1);
  }
  
  .card-title {
    margin: 0;
    font-size: 1.75rem;
    color: #2c3e50;
    font-weight: 600;
  }
  
  .el-form-item {
    margin-bottom: 24px;
  }
  
  .el-button {
    margin-right: 12px;
    transition: all 0.3s ease;
  }
  
  .el-button:hover {
    transform: translateY(-2px);
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  }
  
  .el-input-number {
    width: 100%;
  }
  
  .upload-demo {
    display: flex;
    justify-content: center;
  }
  .action-button {
    margin-right: 12px;
    transition: all 0.3s ease;
  }
  .action-button:hover {
    transform: translateY(-2px);
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  }
  </style>
