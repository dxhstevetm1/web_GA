<template>
  <div class="web-scraping">
    <div class="container">
      <div class="header-section">
        <h2>🕷️ Web Scraping Mode</h2>
        <p class="subtitle">Scrape Facebook post comments directly from web pages (no API token required)</p>
        <div class="mode-toggle">
          <router-link to="/" class="btn btn-outline">📊 API Mode</router-link>
          <span class="btn btn-primary active">🕷️ Web Scraping</span>
        </div>
      </div>

      <div class="input-section">
        <div class="form-group">
          <label for="postUrl">Facebook Post URL:</label>
          <input 
            id="postUrl"
            v-model="postUrl" 
            type="text" 
            placeholder="Paste Facebook post URL here (e.g., https://facebook.com/groups/123/posts/456)"
            class="form-control url-input"
            @paste="handleUrlPaste"
            @blur="validateUrl"
          />
          <div v-if="urlValidation" class="url-validation" :class="urlValidation.isValid ? 'valid' : 'invalid'">
            {{ urlValidation.message }}
          </div>
        </div>

        <div class="options-section">
          <h4>Scraping Options</h4>
          <div class="options-grid">
            <div class="form-group">
              <label for="maxComments">Max Comments:</label>
              <input 
                id="maxComments"
                v-model.number="options.maxComments" 
                type="number" 
                min="10"
                max="5000"
                class="form-control"
              />
            </div>

            <div class="form-group">
              <label for="sortOrder">Sort Order:</label>
              <select id="sortOrder" v-model="options.sortOrder" class="form-control">
                <option value="OldestFirst">Oldest First</option>
                <option value="NewestFirst">Newest First</option>
                <option value="MostRelevant">Most Relevant</option>
              </select>
            </div>

            <div class="form-group checkbox-group">
              <input 
                id="loadReplies"
                v-model="options.loadReplies" 
                type="checkbox"
              />
              <label for="loadReplies">Load Replies</label>
            </div>

            <div class="form-group checkbox-group">
              <input 
                id="loadReactions"
                v-model="options.loadReactions" 
                type="checkbox"
              />
              <label for="loadReactions">Load Reactions</label>
            </div>
          </div>

          <!-- Advanced Filters -->
          <div class="filters-section" v-if="showAdvancedFilters">
            <h5>Advanced Filters</h5>
            <div class="filters-grid">
              <div class="form-group">
                <label for="startDate">From Date:</label>
                <input 
                  id="startDate"
                  v-model="filters.startDate" 
                  type="date"
                  class="form-control"
                />
              </div>

              <div class="form-group">
                <label for="endDate">To Date:</label>
                <input 
                  id="endDate"
                  v-model="filters.endDate" 
                  type="date"
                  class="form-control"
                />
              </div>

              <div class="form-group">
                <label for="minLikes">Min Likes:</label>
                <input 
                  id="minLikes"
                  v-model.number="filters.minLikes" 
                  type="number"
                  min="0"
                  class="form-control"
                />
              </div>

              <div class="form-group checkbox-group">
                <input 
                  id="onlySharers"
                  v-model="filters.onlySharers" 
                  type="checkbox"
                />
                <label for="onlySharers">Only Sharers</label>
              </div>

              <div class="form-group">
                <label for="contentKeywords">Content Keywords:</label>
                <input 
                  id="contentKeywords"
                  v-model="filters.contentKeywords" 
                  type="text"
                  placeholder="keyword1, keyword2"
                  class="form-control"
                />
              </div>

              <div class="form-group">
                <label for="authorNames">Author Names:</label>
                <input 
                  id="authorNames"
                  v-model="filters.authorNames" 
                  type="text"
                  placeholder="John Doe, Jane Smith"
                  class="form-control"
                />
              </div>
            </div>
          </div>

          <div class="toggle-section">
            <button @click="showAdvancedFilters = !showAdvancedFilters" class="btn btn-link">
              {{ showAdvancedFilters ? '🔼 Hide' : '🔽 Show' }} Advanced Filters
            </button>
          </div>
        </div>

        <div class="button-group">
          <button @click="validateAndScrape" :disabled="loading || !postUrl" class="btn btn-primary">
            {{ loading ? 'Scraping...' : '🕷️ Start Scraping' }}
          </button>
          <button @click="getPostInfo" :disabled="loading || !postUrl" class="btn btn-outline">
            {{ loading ? 'Loading...' : '📄 Get Post Info' }}
          </button>
          <button @click="getSharers" :disabled="loading || !postUrl" class="btn btn-success">
            {{ loading ? 'Loading...' : '👥 Get Sharers Only' }}
          </button>
        </div>
      </div>

      <!-- Progress indicator -->
      <div v-if="loading" class="progress-section">
        <div class="progress-card">
          <div class="progress-header">
            <h4>⏳ Scraping in Progress...</h4>
            <p>{{ currentOperation }}</p>
          </div>
          <div class="progress-bar">
            <div class="progress-fill" :style="{ width: progress + '%' }"></div>
          </div>
          <p class="progress-text">{{ progress }}% complete</p>
          <small>This may take 1-3 minutes depending on the number of comments</small>
        </div>
      </div>

      <!-- Error display -->
      <div v-if="error" class="error-section">
        <div class="error-card">
          <h4>❌ Error</h4>
          <p>{{ error }}</p>
          <button @click="error = null" class="btn btn-link">Dismiss</button>
        </div>
      </div>

      <!-- Post Information -->
      <div v-if="scrapedData?.postInfo" class="post-section">
        <h3>📄 Post Information</h3>
        <div class="post-card">
          <div class="post-header">
            <div class="post-meta">
              <h4>{{ scrapedData.postInfo.authorName }}</h4>
              <p>{{ formatDate(scrapedData.postInfo.postTime) }}</p>
              <p v-if="scrapedData.postInfo.isGroupPost" class="group-info">
                <strong>Group:</strong> {{ scrapedData.postInfo.groupName }}
              </p>
            </div>
          </div>
          <div class="post-content">
            <p>{{ scrapedData.postInfo.content }}</p>
          </div>
          <div class="post-stats">
            <span>❤️ {{ scrapedData.postInfo.likesCount }}</span>
            <span>💬 {{ scrapedData.postInfo.commentsCount }}</span>
            <span>🔄 {{ scrapedData.postInfo.sharesCount }}</span>
          </div>
        </div>
      </div>

      <!-- Comments Section -->
      <div v-if="comments.length > 0" class="comments-section">
        <div class="comments-header">
          <h3>💬 Comments ({{ comments.length }})</h3>
          <div class="stats-row">
            <span class="stat">Total: {{ scrapedData?.totalCommentsScraped || comments.length }}</span>
            <span v-if="scrapedData?.sharersCount" class="stat shared">
              Sharers: {{ scrapedData.sharersCount }}
            </span>
            <span v-if="scrapedData?.groupMembersCount" class="stat members">
              Group Members: {{ scrapedData.groupMembersCount }}
            </span>
          </div>
          <div class="search-section">
            <input 
              v-model="searchTerm" 
              type="text" 
              placeholder="🔍 Search comments..." 
              class="search-input"
            />
            <select v-model="sortBy" class="sort-select">
              <option value="commentTime">📅 Date</option>
              <option value="likesCount">❤️ Likes</option>
              <option value="authorName">👤 Author</option>
              <option value="hasSharedPost">🔄 Shared Status</option>
            </select>
          </div>
        </div>

        <div class="comments-list">
          <div 
            v-for="comment in filteredComments" 
            :key="comment.commentId"
            class="comment-card"
            :class="{ 'shared': comment.hasSharedPost, 'group-member': comment.isGroupMember }"
          >
            <div class="comment-header">
              <div class="comment-meta">
                <h5>{{ comment.authorName }}</h5>
                <p class="comment-time">{{ formatDate(comment.commentTime) }}</p>
                <div class="comment-badges">
                  <span v-if="comment.hasSharedPost" class="badge shared">🔄 Shared</span>
                  <span v-if="comment.isGroupMember" class="badge member">👥 Member</span>
                  <span v-if="comment.groupRole === 'admin'" class="badge admin">👑 Admin</span>
                  <span v-if="comment.groupRole === 'moderator'" class="badge mod">🛡️ Mod</span>
                </div>
              </div>
              <div class="comment-stats">
                <span>❤️ {{ comment.likesCount }}</span>
                <span v-if="comment.repliesCount">💬 {{ comment.repliesCount }}</span>
              </div>
            </div>
            <div class="comment-content">
              <p>{{ comment.content }}</p>
            </div>
            <div v-if="comment.hasSharedPost && comment.shareUrl" class="share-info">
              <p><strong>🔄 Share:</strong> <a :href="comment.shareUrl" target="_blank">View Share</a></p>
              <p v-if="comment.shareTime"><strong>⏰ Shared:</strong> {{ formatDate(comment.shareTime) }}</p>
            </div>
          </div>
        </div>

        <!-- Export Options -->
        <div class="export-section">
          <h4>📥 Export Data</h4>
          <div class="export-buttons">
            <button @click="exportToJSON" class="btn btn-outline">📄 Export JSON</button>
            <button @click="exportToCSV" class="btn btn-outline">📊 Export CSV</button>
            <button @click="exportSharersOnly" class="btn btn-success">👥 Export Sharers Only</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { webScrapingService, SortOrder, defaultScrapingOptions } from '../services/webScrapingService.js'

export default {
  name: 'WebScrapingView',
  data() {
    return {
      postUrl: '',
      urlValidation: null,
      loading: false,
      error: null,
      currentOperation: '',
      progress: 0,
      showAdvancedFilters: false,
      
      options: {
        maxComments: defaultScrapingOptions.maxComments,
        sortOrder: defaultScrapingOptions.sortOrder,
        loadReplies: defaultScrapingOptions.loadReplies,
        loadReactions: defaultScrapingOptions.loadReactions
      },
      
      filters: {
        startDate: null,
        endDate: null,
        minLikes: null,
        onlySharers: false,
        onlyGroupMembers: false,
        contentKeywords: '',
        authorNames: ''
      },
      
      scrapedData: null,
      comments: [],
      searchTerm: '',
      sortBy: 'commentTime'
    }
  },
  
  computed: {
    filteredComments() {
      let filtered = [...this.comments]
      
      // Search filter
      if (this.searchTerm) {
        const term = this.searchTerm.toLowerCase()
        filtered = filtered.filter(comment => 
          comment.content.toLowerCase().includes(term) ||
          comment.authorName.toLowerCase().includes(term)
        )
      }
      
      // Sort
      filtered.sort((a, b) => {
        const aVal = a[this.sortBy]
        const bVal = b[this.sortBy]
        
        if (this.sortBy === 'commentTime') {
          return new Date(bVal) - new Date(aVal) // Newest first
        } else if (typeof aVal === 'number') {
          return bVal - aVal // Descending for numbers
        } else {
          return String(aVal).localeCompare(String(bVal))
        }
      })
      
      return filtered
    }
  },
  
  methods: {
    async handleUrlPaste(event) {
      // Auto-validate after paste
      setTimeout(() => {
        this.validateUrl()
      }, 100)
    },
    
    async validateUrl() {
      if (!this.postUrl) {
        this.urlValidation = null
        return
      }
      
      try {
        const result = await webScrapingService.validateUrl(this.postUrl)
        this.urlValidation = {
          isValid: result.isValid,
          message: result.message,
          urlInfo: result.urlInfo
        }
      } catch (error) {
        this.urlValidation = {
          isValid: false,
          message: 'Error validating URL: ' + error.message
        }
      }
    },
    
    async validateAndScrape() {
      await this.validateUrl()
      if (this.urlValidation?.isValid) {
        await this.scrapeComments()
      } else {
        this.error = 'Please enter a valid Facebook post URL'
      }
    },
    
    async scrapeComments() {
      this.loading = true
      this.error = null
      this.currentOperation = 'Initializing browser...'
      this.progress = 10
      
      try {
        // Simulate progress updates
        const progressUpdates = [
          { operation: 'Loading Facebook page...', progress: 25 },
          { operation: 'Scrolling to load comments...', progress: 50 },
          { operation: 'Extracting comment data...', progress: 75 },
          { operation: 'Checking share status...', progress: 90 },
          { operation: 'Finalizing...', progress: 95 }
        ]
        
        // Start the scraping
        const scrapePromise = webScrapingService.analyzeComments(this.postUrl, {
          ...this.options,
          filters: this.filters
        })
        
        // Update progress
        for (const update of progressUpdates) {
          await new Promise(resolve => setTimeout(resolve, 2000))
          this.currentOperation = update.operation
          this.progress = update.progress
        }
        
        const result = await scrapePromise
        
        if (result.success) {
          this.scrapedData = result.data
          this.comments = result.data.comments || []
          this.progress = 100
        } else {
          throw new Error(result.error || 'Scraping failed')
        }
        
      } catch (error) {
        this.error = 'Scraping failed: ' + error.message
        console.error('Scraping error:', error)
      } finally {
        this.loading = false
        this.currentOperation = ''
        this.progress = 0
      }
    },
    
    async getPostInfo() {
      this.loading = true
      this.error = null
      
      try {
        const result = await webScrapingService.getPostInfo(this.postUrl)
        
        if (result.success) {
          this.scrapedData = { postInfo: result.data }
        } else {
          throw new Error(result.error || 'Failed to get post info')
        }
        
      } catch (error) {
        this.error = 'Failed to get post info: ' + error.message
      } finally {
        this.loading = false
      }
    },
    
    async getSharers() {
      this.loading = true
      this.error = null
      
      try {
        const result = await webScrapingService.getSharers(this.postUrl, this.options)
        
        if (result.success) {
          this.comments = result.data || []
          this.scrapedData = {
            totalCommentsScraped: result.totalComments,
            sharersCount: result.totalSharers
          }
        } else {
          throw new Error(result.error || 'Failed to get sharers')
        }
        
      } catch (error) {
        this.error = 'Failed to get sharers: ' + error.message
      } finally {
        this.loading = false
      }
    },
    
    formatDate(dateString) {
      if (!dateString) return 'Unknown'
      const date = new Date(dateString)
      return date.toLocaleString()
    },
    
    exportToJSON() {
      const data = {
        postInfo: this.scrapedData?.postInfo,
        comments: this.filteredComments,
        exportedAt: new Date().toISOString()
      }
      
      this.downloadFile(
        JSON.stringify(data, null, 2),
        'facebook-comments.json',
        'application/json'
      )
    },
    
    exportToCSV() {
      const headers = ['Author', 'Content', 'Date', 'Likes', 'Shared', 'Profile URL']
      const rows = this.filteredComments.map(comment => [
        comment.authorName,
        `"${comment.content.replace(/"/g, '""')}"`,
        comment.commentTime,
        comment.likesCount,
        comment.hasSharedPost ? 'Yes' : 'No',
        comment.authorProfileUrl
      ])
      
      const csvContent = [headers, ...rows]
        .map(row => row.join(','))
        .join('\n')
      
      this.downloadFile(csvContent, 'facebook-comments.csv', 'text/csv')
    },
    
    exportSharersOnly() {
      const sharers = this.filteredComments.filter(c => c.hasSharedPost)
      const data = {
        postUrl: this.postUrl,
        totalSharers: sharers.length,
        sharers: sharers,
        exportedAt: new Date().toISOString()
      }
      
      this.downloadFile(
        JSON.stringify(data, null, 2),
        'facebook-sharers.json',
        'application/json'
      )
    },
    
    downloadFile(content, filename, mimeType) {
      const blob = new Blob([content], { type: mimeType })
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = filename
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      URL.revokeObjectURL(url)
    }
  }
}
</script>

<style scoped>
.web-scraping {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  background-attachment: fixed;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem 1rem;
}

.header-section {
  text-align: center;
  margin-bottom: 3rem;
  color: white;
}

.header-section h2 {
  font-size: 2.5rem;
  margin-bottom: 0.5rem;
  text-shadow: 0 2px 4px rgba(0,0,0,0.3);
}

.subtitle {
  font-size: 1.2rem;
  opacity: 0.9;
  margin-bottom: 2rem;
}

.mode-toggle {
  display: flex;
  gap: 1rem;
  justify-content: center;
}

.mode-toggle .btn {
  padding: 0.75rem 1.5rem;
  text-decoration: none;
  border-radius: 25px;
  transition: all 0.3s ease;
}

.mode-toggle .active {
  background: white;
  color: #667eea;
  box-shadow: 0 4px 15px rgba(0,0,0,0.2);
}

.input-section {
  background: white;
  padding: 2rem;
  border-radius: 15px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.2);
  margin-bottom: 2rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  font-weight: 600;
  margin-bottom: 0.5rem;
  color: #333;
}

.form-control {
  width: 100%;
  padding: 0.75rem;
  border: 2px solid #e9ecef;
  border-radius: 8px;
  font-size: 1rem;
  transition: border-color 0.3s ease;
}

.form-control:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.url-input {
  font-family: monospace;
  font-size: 0.9rem;
}

.url-validation {
  margin-top: 0.5rem;
  padding: 0.5rem;
  border-radius: 5px;
  font-size: 0.9rem;
}

.url-validation.valid {
  background: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

.url-validation.invalid {
  background: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

.options-section {
  margin-top: 2rem;
  padding-top: 2rem;
  border-top: 1px solid #dee2e6;
}

.options-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.checkbox-group {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.checkbox-group input[type="checkbox"] {
  width: auto;
}

.filters-section {
  margin-top: 1.5rem;
  padding: 1.5rem;
  background: #f8f9fa;
  border-radius: 8px;
}

.filters-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

.toggle-section {
  text-align: center;
  margin-top: 1rem;
}

.button-group {
  display: flex;
  gap: 1rem;
  justify-content: center;
  flex-wrap: wrap;
  margin-top: 2rem;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  text-decoration: none;
  display: inline-block;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(102, 126, 234, 0.4);
}

.btn-success {
  background: linear-gradient(135deg, #56ab2f 0%, #a8e6cf 100%);
  color: white;
}

.btn-outline {
  background: transparent;
  border: 2px solid #667eea;
  color: #667eea;
}

.btn-outline:hover:not(:disabled) {
  background: #667eea;
  color: white;
}

.btn-link {
  background: none;
  border: none;
  color: #667eea;
  text-decoration: underline;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.progress-section {
  margin: 2rem 0;
}

.progress-card {
  background: white;
  padding: 2rem;
  border-radius: 15px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.1);
  text-align: center;
}

.progress-bar {
  width: 100%;
  height: 20px;
  background: #e9ecef;
  border-radius: 10px;
  overflow: hidden;
  margin: 1rem 0;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, #667eea, #764ba2);
  transition: width 0.3s ease;
}

.error-section {
  margin: 2rem 0;
}

.error-card {
  background: #f8d7da;
  color: #721c24;
  padding: 1.5rem;
  border-radius: 8px;
  border: 1px solid #f5c6cb;
}

.post-section, .comments-section {
  background: white;
  padding: 2rem;
  border-radius: 15px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.1);
  margin-bottom: 2rem;
}

.post-card {
  border: 1px solid #dee2e6;
  border-radius: 8px;
  padding: 1.5rem;
}

.post-header {
  display: flex;
  align-items: center;
  margin-bottom: 1rem;
}

.post-meta h4 {
  margin: 0 0 0.5rem 0;
  color: #333;
}

.post-stats {
  display: flex;
  gap: 1rem;
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #dee2e6;
}

.comments-header {
  margin-bottom: 2rem;
}

.stats-row {
  display: flex;
  gap: 1rem;
  margin: 1rem 0;
}

.stat {
  padding: 0.5rem 1rem;
  background: #f8f9fa;
  border-radius: 20px;
  font-size: 0.9rem;
}

.stat.shared {
  background: #d1ecf1;
  color: #0c5460;
}

.stat.members {
  background: #d4edda;
  color: #155724;
}

.search-section {
  display: flex;
  gap: 1rem;
  margin-top: 1rem;
}

.search-input, .sort-select {
  flex: 1;
  padding: 0.5rem;
  border: 1px solid #dee2e6;
  border-radius: 5px;
}

.comments-list {
  max-height: 600px;
  overflow-y: auto;
}

.comment-card {
  border: 1px solid #dee2e6;
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 1rem;
  transition: all 0.3s ease;
}

.comment-card:hover {
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.comment-card.shared {
  border-left: 4px solid #28a745;
}

.comment-card.group-member {
  background: #f8f9ff;
}

.comment-header {
  display: flex;
  justify-content: between;
  align-items: flex-start;
  margin-bottom: 0.5rem;
}

.comment-meta h5 {
  margin: 0 0 0.25rem 0;
  color: #333;
}

.comment-time {
  margin: 0;
  font-size: 0.8rem;
  color: #666;
}

.comment-badges {
  display: flex;
  gap: 0.5rem;
  margin-top: 0.5rem;
}

.badge {
  padding: 0.2rem 0.5rem;
  border-radius: 12px;
  font-size: 0.7rem;
  font-weight: 600;
}

.badge.shared {
  background: #d1ecf1;
  color: #0c5460;
}

.badge.member {
  background: #d4edda;
  color: #155724;
}

.badge.admin {
  background: #fff3cd;
  color: #856404;
}

.badge.mod {
  background: #cce5ff;
  color: #004085;
}

.comment-stats {
  font-size: 0.8rem;
  color: #666;
}

.comment-content {
  margin: 0.5rem 0;
  line-height: 1.5;
}

.share-info {
  background: #f8f9fa;
  padding: 0.5rem;
  border-radius: 5px;
  margin-top: 0.5rem;
  font-size: 0.9rem;
}

.export-section {
  margin-top: 2rem;
  padding-top: 2rem;
  border-top: 1px solid #dee2e6;
  text-align: center;
}

.export-buttons {
  display: flex;
  gap: 1rem;
  justify-content: center;
  flex-wrap: wrap;
}

@media (max-width: 768px) {
  .container {
    padding: 1rem;
  }
  
  .header-section h2 {
    font-size: 2rem;
  }
  
  .input-section {
    padding: 1.5rem;
  }
  
  .options-grid {
    grid-template-columns: 1fr;
  }
  
  .button-group {
    flex-direction: column;
  }
  
  .search-section {
    flex-direction: column;
  }
  
  .export-buttons {
    flex-direction: column;
  }
}
</style>