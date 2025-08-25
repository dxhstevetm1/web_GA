<template>
  <div class="home">
    <div class="container">
      <div class="header-section">
        <h2>📊 Facebook Comment Analyzer</h2>
        <p class="subtitle">Analyze comments from Facebook posts and track user engagement</p>
        <div class="mode-toggle">
          <span class="btn btn-primary active">📊 API Mode</span>
          <router-link to="/scraping" class="btn btn-outline">🕷️ Web Scraping</router-link>
        </div>
        <div class="mode-info">
          <p><strong>API Mode:</strong> Uses Facebook Graph API (requires access token)</p>
        </div>
      </div>

      <div class="input-section">
        <div class="form-group">
          <label for="postType">Post Type:</label>
          <select id="postType" v-model="postType" class="form-control">
            <option value="regular">Regular Post</option>
            <option value="group">Group Post</option>
          </select>
        </div>

        <div class="form-group">
          <label for="postId">Facebook Post ID:</label>
          <input 
            id="postId"
            v-model="postId" 
            type="text" 
            :placeholder="postType === 'group' ? 'Enter Facebook group post ID (e.g., 123456789_987654321)' : 'Enter Facebook post ID (e.g., 123456789_987654321)'"
            class="form-control"
          />
        </div>

        <div class="form-group">
          <label for="accessToken">Access Token (Optional if configured in backend):</label>
          <input 
            id="accessToken"
            v-model="accessToken" 
            type="password" 
            placeholder="Enter Facebook access token (optional)"
            class="form-control"
          />
          <small class="form-text">Leave empty if access token is configured in backend</small>
        </div>

        <div class="button-group">
          <button @click="getPost" :disabled="loading" class="btn btn-primary">
            {{ loading ? 'Loading...' : 'Get Post Info' }}
          </button>
          <button @click="analyzeComments" :disabled="loading || !postId" class="btn btn-success">
            {{ loading ? 'Analyzing...' : 'Analyze Comments' }}
          </button>
        </div>
      </div>

      <!-- Post Information -->
      <div v-if="post" class="post-section">
        <h3>Post Information</h3>
        <div class="post-card">
          <div class="post-header">
            <img :src="post.from.picture?.data?.url || '/default-avatar.png'" alt="Profile" class="profile-pic" />
            <div class="post-meta">
              <h4>{{ post.from.name }}</h4>
              <p>{{ formatDate(post.created_time) }}</p>
              <p v-if="post.group" class="group-info">
                <strong>Group:</strong> {{ post.group.name }} ({{ post.group.privacy }})
              </p>
            </div>
          </div>
          <div class="post-content">
            <p>{{ post.message }}</p>
          </div>
          <div class="post-stats">
            <span>Likes: {{ post.likes?.data?.length || 0 }}</span>
            <span>Comments: {{ comments.length }}</span>
            <span>Shares: {{ post.shares || 0 }}</span>
            <span v-if="post.type">Type: {{ post.type }}</span>
          </div>
        </div>
      </div>

      <!-- Comments Section -->
      <div v-if="comments.length > 0" class="comments-section">
        <div class="comments-header">
          <h3>Comments ({{ comments.length }})</h3>
          <div class="filters">
            <input 
              v-model="searchTerm" 
              type="text" 
              placeholder="Search comments..." 
              class="search-input"
            />
            <select v-model="sortBy" class="sort-select">
              <option value="created_time">Sort by Date</option>
              <option value="like_count">Sort by Likes</option>
              <option value="hasSharedPost">Sort by Shared</option>
              <option value="groupRole">Sort by Group Role</option>
            </select>
            <select v-model="filterBy" class="filter-select">
              <option value="all">All Comments</option>
              <option value="shared">Shared Post</option>
              <option value="not_shared">Not Shared</option>
              <option value="group_members">Group Members Only</option>
            </select>
          </div>
        </div>

        <div class="comments-list">
          <div 
            v-for="comment in filteredAndSortedComments" 
            :key="comment.id" 
            class="comment-card"
            :class="{ 
              'shared-post': comment.hasSharedPost,
              'group-member': comment.isGroupMember 
            }"
          >
            <div class="comment-header">
              <img :src="comment.from.picture?.data?.url || '/default-avatar.png'" alt="Profile" class="profile-pic" />
              <div class="comment-meta">
                <h5>{{ comment.from.name }}</h5>
                <p>{{ formatDate(comment.created_time) }}</p>
                <div class="comment-badges">
                  <span v-if="comment.hasSharedPost" class="badge shared">Shared Post</span>
                  <span v-if="comment.isGroupMember" class="badge member">Group Member</span>
                  <span v-if="comment.groupRole" class="badge role">{{ comment.groupRole }}</span>
                </div>
              </div>
            </div>
            <div class="comment-content">
              <p>{{ comment.message }}</p>
              <div v-if="comment.hasSharedPost && comment.shareMessage" class="share-info">
                <small><strong>Share Message:</strong> {{ comment.shareMessage }}</small>
                <br>
                <small><strong>Share Type:</strong> {{ comment.shareType }}</small>
                <br>
                <small v-if="comment.shareTime"><strong>Share Time:</strong> {{ formatDate(comment.shareTime) }}</small>
              </div>
            </div>
            <div class="comment-stats">
              <span>👍 {{ comment.like_count }}</span>
              <span>💬 {{ comment.comment_count }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Loading and Error States -->
      <div v-if="error" class="error-message">
        <p>{{ error }}</p>
      </div>

      <!-- Configuration Status -->
      <div v-if="config" class="config-section">
        <h4>API Configuration Status</h4>
        <div class="config-info">
          <p><strong>Base URL:</strong> {{ config.baseUrl }}</p>
          <p><strong>Access Token:</strong> {{ config.hasAccessToken ? 'Configured' : 'Not configured' }}</p>
          <p><strong>App ID:</strong> {{ config.hasAppId ? 'Configured' : 'Not configured' }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted } from 'vue'
import { facebookService } from '../services/facebookService'

export default {
  name: 'HomeView',
  setup() {
    const postType = ref('regular')
    const postId = ref('')
    const accessToken = ref('')
    const post = ref(null)
    const comments = ref([])
    const loading = ref(false)
    const error = ref('')
    const searchTerm = ref('')
    const sortBy = ref('created_time')
    const filterBy = ref('all')
    const config = ref(null)

    const filteredAndSortedComments = computed(() => {
      let filtered = comments.value

      // Filter by search term
      if (searchTerm.value) {
        filtered = filtered.filter(comment => 
          comment.message.toLowerCase().includes(searchTerm.value.toLowerCase()) ||
          comment.from.name.toLowerCase().includes(searchTerm.value.toLowerCase())
        )
      }

      // Filter by type
      switch (filterBy.value) {
        case 'shared':
          filtered = filtered.filter(comment => comment.hasSharedPost)
          break
        case 'not_shared':
          filtered = filtered.filter(comment => !comment.hasSharedPost)
          break
        case 'group_members':
          filtered = filtered.filter(comment => comment.isGroupMember)
          break
      }

      // Sort comments
      return filtered.sort((a, b) => {
        switch (sortBy.value) {
          case 'created_time':
            return new Date(a.created_time) - new Date(b.created_time)
          case 'like_count':
            return b.like_count - a.like_count
          case 'hasSharedPost':
            return b.hasSharedPost - a.hasSharedPost
          case 'groupRole':
            const roleOrder = { admin: 3, moderator: 2, member: 1 }
            return (roleOrder[b.groupRole] || 0) - (roleOrder[a.groupRole] || 0)
          default:
            return 0
        }
      })
    })

    const getPost = async () => {
      if (!postId.value) {
        error.value = 'Please enter Post ID'
        return
      }

      loading.value = true
      error.value = ''

      try {
        let response
        if (postType.value === 'group') {
          response = await facebookService.getGroupPost(postId.value, accessToken.value || null)
        } else {
          response = await facebookService.getPost(postId.value, accessToken.value || null)
        }
        post.value = response
      } catch (err) {
        error.value = err.message || 'Failed to get post information'
      } finally {
        loading.value = false
      }
    }

    const analyzeComments = async () => {
      if (!postId.value) {
        error.value = 'Please enter Post ID'
        return
      }

      loading.value = true
      error.value = ''

      try {
        let response
        if (postType.value === 'group') {
          response = await facebookService.analyzeGroupPostComments(postId.value, accessToken.value || null)
        } else {
          response = await facebookService.analyzeComments(postId.value, accessToken.value || null)
        }
        comments.value = response
      } catch (err) {
        error.value = err.message || 'Failed to analyze comments'
      } finally {
        loading.value = false
      }
    }

    const formatDate = (dateString) => {
      return new Date(dateString).toLocaleString()
    }

    const loadConfig = async () => {
      try {
        config.value = await facebookService.getConfig()
      } catch (err) {
        console.error('Failed to load config:', err)
      }
    }

    onMounted(() => {
      loadConfig()
    })

    return {
      postType,
      postId,
      accessToken,
      post,
      comments,
      loading,
      error,
      searchTerm,
      sortBy,
      filterBy,
      config,
      filteredAndSortedComments,
      getPost,
      analyzeComments,
      formatDate
    }
  }
}
</script>

<style scoped>
.home {
  padding: 2rem 0;
}

.header-section {
  text-align: center;
  margin-bottom: 3rem;
}

.header-section h2 {
  color: #2c3e50;
  margin-bottom: 0.5rem;
}

.subtitle {
  color: #7f8c8d;
  font-size: 1.1rem;
}

.input-section {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
  margin-bottom: 2rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: #2c3e50;
}

.form-control {
  width: 100%;
  padding: 0.75rem;
  border: 2px solid #e1e8ed;
  border-radius: 4px;
  font-size: 1rem;
  transition: border-color 0.3s;
}

.form-control:focus {
  outline: none;
  border-color: #667eea;
}

.form-text {
  color: #7f8c8d;
  font-size: 0.9rem;
  margin-top: 0.25rem;
}

.button-group {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  background: #667eea;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #5a6fd8;
}

.btn-success {
  background: #28a745;
  color: white;
}

.btn-success:hover:not(:disabled) {
  background: #218838;
}

.post-section, .comments-section, .config-section {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
  margin-bottom: 2rem;
}

.post-card {
  border: 1px solid #e1e8ed;
  border-radius: 8px;
  padding: 1.5rem;
}

.post-header {
  display: flex;
  align-items: center;
  margin-bottom: 1rem;
}

.profile-pic {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  margin-right: 1rem;
  object-fit: cover;
}

.post-meta h4 {
  margin: 0;
  color: #2c3e50;
}

.post-meta p {
  margin: 0;
  color: #7f8c8d;
  font-size: 0.9rem;
}

.group-info {
  color: #667eea !important;
  font-weight: 500;
}

.post-content {
  margin-bottom: 1rem;
}

.post-stats {
  display: flex;
  gap: 1rem;
  color: #7f8c8d;
  font-size: 0.9rem;
  flex-wrap: wrap;
}

.comments-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  flex-wrap: wrap;
  gap: 1rem;
}

.filters {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.search-input, .sort-select, .filter-select {
  padding: 0.5rem;
  border: 1px solid #e1e8ed;
  border-radius: 4px;
  font-size: 0.9rem;
}

.comments-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.comment-card {
  border: 1px solid #e1e8ed;
  border-radius: 8px;
  padding: 1.5rem;
  transition: all 0.3s;
}

.comment-card:hover {
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.comment-card.shared-post {
  border-left: 4px solid #28a745;
  background-color: #f8fff9;
}

.comment-card.group-member {
  border-right: 4px solid #667eea;
}

.comment-header {
  display: flex;
  align-items: center;
  margin-bottom: 1rem;
  position: relative;
}

.comment-meta h5 {
  margin: 0;
  color: #2c3e50;
}

.comment-meta p {
  margin: 0;
  color: #7f8c8d;
  font-size: 0.9rem;
}

.comment-badges {
  display: flex;
  gap: 0.5rem;
  margin-top: 0.5rem;
  flex-wrap: wrap;
}

.badge {
  padding: 0.25rem 0.5rem;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 600;
}

.badge.shared {
  background: #28a745;
  color: white;
}

.badge.member {
  background: #667eea;
  color: white;
}

.badge.role {
  background: #ffc107;
  color: #212529;
}

.comment-content {
  margin-bottom: 1rem;
}

.comment-content p {
  margin: 0;
  line-height: 1.5;
}

.share-info {
  margin-top: 0.5rem;
  padding: 0.5rem;
  background: #f8f9fa;
  border-radius: 4px;
  border-left: 3px solid #28a745;
}

.comment-stats {
  display: flex;
  gap: 1rem;
  color: #7f8c8d;
  font-size: 0.9rem;
}

.error-message {
  background: #f8d7da;
  color: #721c24;
  padding: 1rem;
  border-radius: 4px;
  border: 1px solid #f5c6cb;
  margin-top: 1rem;
}

.config-info {
  background: #f8f9fa;
  padding: 1rem;
  border-radius: 4px;
}

.config-info p {
  margin: 0.5rem 0;
}

.mode-toggle {
  display: flex;
  gap: 1rem;
  justify-content: center;
  margin: 1.5rem 0;
}

.mode-toggle .btn {
  padding: 0.75rem 1.5rem;
  text-decoration: none;
  border-radius: 25px;
  transition: all 0.3s ease;
  border: 2px solid transparent;
}

.mode-toggle .active {
  background: #667eea;
  color: white;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
}

.mode-toggle .btn-outline {
  background: transparent;
  border: 2px solid #667eea;
  color: #667eea;
}

.mode-toggle .btn-outline:hover {
  background: #667eea;
  color: white;
}

.mode-info {
  text-align: center;
  margin-top: 1rem;
  opacity: 0.8;
}

@media (max-width: 768px) {
  .button-group {
    flex-direction: column;
  }
  
  .comments-header {
    flex-direction: column;
    align-items: stretch;
  }
  
  .filters {
    flex-direction: column;
  }
  
  .post-stats {
    flex-direction: column;
    gap: 0.5rem;
  }
}
</style>