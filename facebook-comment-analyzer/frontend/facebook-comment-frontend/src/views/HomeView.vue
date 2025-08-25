<template>
  <div class="home">
    <div class="container">
      <div class="header-section">
        <h2>Facebook Comment Analyzer</h2>
        <p class="subtitle">Analyze comments from Facebook posts and track user engagement</p>
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

        <div class="form-group">
          <label for="postUrl">Public Post URL (no token needed):</label>
          <input 
            id="postUrl"
            v-model="postUrl" 
            type="text" 
            placeholder="https://www.facebook.com/... or https://www.facebook.com/groups/.../permalink/..."
            class="form-control"
          />
          <div style="margin-top:0.5rem; display:flex; gap:0.5rem; align-items:center;">
            <input id="checkShare" type="checkbox" v-model="checkShare" />
            <label for="checkShare">Try detect if commenters shared the post</label>
          </div>
        </div>

        <div class="button-group">
          <button @click="getPost" :disabled="loading" class="btn btn-primary">
            {{ loading ? 'Loading...' : 'Get Post Info' }}
          </button>
          <button @click="analyzeComments" :disabled="loading || !postId" class="btn btn-success">
            {{ loading ? 'Analyzing...' : 'Analyze Comments' }}
          </button>
          <button @click="scrapeComments" :disabled="loading || !postUrl" class="btn btn-secondary">
            {{ loading ? 'Scraping...' : 'Scrape Comments by URL' }}
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
            :key="comment.id || (comment.from.name + comment.created_time)" 
            class="comment-card"
            :class="{ 
              'shared-post': comment.hasSharedPost,
              'group-member': comment.isGroupMember 
            }"
          >
            <div class="comment-header">
              <img :src="comment.from.picture?.data?.url || '/default-avatar.png'" alt="Profile" class="profile-pic" />
              <div class="comment-meta">
                <h5>
                  <a v-if="comment.from.profileUrl" :href="comment.from.profileUrl" target="_blank" rel="noopener">{{ comment.from.name }}</a>
                  <span v-else>{{ comment.from.name }}</span>
                </h5>
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
    const postUrl = ref('')
    const checkShare = ref(false)
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
          (comment.message || '').toLowerCase().includes(searchTerm.value.toLowerCase()) ||
          (comment.from?.name || '').toLowerCase().includes(searchTerm.value.toLowerCase())
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
            return (b.like_count || 0) - (a.like_count || 0)
          case 'hasSharedPost':
            return (b.hasSharedPost ? 1 : 0) - (a.hasSharedPost ? 1 : 0)
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

    const scrapeComments = async () => {
      if (!postUrl.value) {
        error.value = 'Please enter Public Post URL'
        return
      }
      loading.value = true
      error.value = ''
      post.value = null
      try {
        const response = await facebookService.scrapeComments(postUrl.value, checkShare.value)
        comments.value = response
      } catch (err) {
        error.value = err.message || 'Failed to scrape comments'
      } finally {
        loading.value = false
      }
    }

    const formatDate = (dateString) => {
      try {
        return new Date(dateString).toLocaleString()
      } catch {
        return String(dateString)
      }
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
      postUrl,
      checkShare,
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
      scrapeComments,
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

.btn-secondary {
  background: #6c757d;
  color: white;
}

.btn-success:hover:not(:disabled) {
  background: #218838;
}

.post-section { margin-bottom: 1.5rem; }
.post-card { background: #fff; border: 1px solid #eef2f7; border-radius: 8px; padding: 1rem; }
.post-header { display: flex; gap: 0.75rem; align-items: center; }
.post-stats span { margin-right: 0.75rem; }

.comments-section {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.08);
}

.comments-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1rem;
}

.filters {
  display: flex;
  gap: 0.75rem;
}

.search-input, .sort-select, .filter-select {
  padding: 0.5rem 0.75rem;
  border: 1px solid #e1e8ed;
  border-radius: 4px;
}

.comments-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 1rem;
}

.comment-card {
  border: 1px solid #eef2f7;
  border-radius: 8px;
  padding: 1rem;
  background: #fff;
}

.comment-card.shared-post {
  border-color: #d1f7c4;
}

.comment-card.group-member {
  border-color: #cfe4ff;
}

.comment-header {
  display: flex;
  gap: 0.75rem;
  align-items: center;
}

.profile-pic {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  object-fit: cover;
}

.comment-meta h5 {
  margin: 0;
}

.badge {
  display: inline-block;
  padding: 0.15rem 0.4rem;
  border-radius: 4px;
  font-size: 0.75rem;
  margin-right: 0.4rem;
}

.badge.shared { background: #e8f5e9; color: #2e7d32; }
.badge.member { background: #e3f2fd; color: #1565c0; }
.badge.role { background: #fff8e1; color: #ef6c00; }

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