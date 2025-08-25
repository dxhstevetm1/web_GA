import axios from 'axios'

const API_BASE_URL = '/api/facebook'

// Create axios instance
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000, // 30 seconds
  headers: {
    'Content-Type': 'application/json'
  }
})

// Request interceptor to add access token
apiClient.interceptors.request.use(
  (config) => {
    // Add access token to query params if available
    if (config.params && config.params.accessToken) {
      config.params.access_token = config.params.accessToken
      delete config.params.accessToken
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor to handle errors
apiClient.interceptors.response.use(
  (response) => {
    return response.data
  },
  (error) => {
    if (error.response) {
      // Server responded with error status
      const message = error.response.data?.message || error.response.data || 'An error occurred'
      throw new Error(message)
    } else if (error.request) {
      // Request was made but no response received
      throw new Error('No response from server. Please check your connection.')
    } else {
      // Something else happened
      throw new Error('An unexpected error occurred.')
    }
  }
)

export const facebookService = {
  /**
   * Get Facebook post information
   * @param {string} postId - Facebook post ID
   * @param {string} accessToken - Facebook access token (optional if configured in backend)
   * @returns {Promise<Object>} Post information
   */
  async getPost(postId, accessToken = null) {
    const params = accessToken ? { accessToken } : {}
    return await apiClient.get(`/post/${postId}`, { params })
  },

  /**
   * Get Facebook group post information
   * @param {string} postId - Facebook group post ID
   * @param {string} accessToken - Facebook access token (optional if configured in backend)
   * @returns {Promise<Object>} Group post information
   */
  async getGroupPost(postId, accessToken = null) {
    const params = accessToken ? { accessToken } : {}
    return await apiClient.get(`/group-post/${postId}`, { params })
  },

  /**
   * Get all comments for a Facebook post
   * @param {string} postId - Facebook post ID
   * @param {string} accessToken - Facebook access token (optional if configured in backend)
   * @returns {Promise<Array>} Array of comments
   */
  async getComments(postId, accessToken = null) {
    const params = accessToken ? { accessToken } : {}
    return await apiClient.get(`/post/${postId}/comments`, { params })
  },

  /**
   * Get all comments for a Facebook group post
   * @param {string} postId - Facebook group post ID
   * @param {string} accessToken - Facebook access token (optional if configured in backend)
   * @returns {Promise<Array>} Array of comments with group info
   */
  async getGroupPostComments(postId, accessToken = null) {
    const params = accessToken ? { accessToken } : {}
    return await apiClient.get(`/group-post/${postId}/comments`, { params })
  },

  /**
   * Analyze comments and check if users shared the post
   * @param {string} postId - Facebook post ID
   * @param {string} accessToken - Facebook access token (optional if configured in backend)
   * @returns {Promise<Array>} Array of analyzed comments
   */
  async analyzeComments(postId, accessToken = null) {
    const params = accessToken ? { accessToken } : {}
    return await apiClient.get(`/post/${postId}/analyze`, { params })
  },

  /**
   * Analyze group post comments and check if users shared the post
   * @param {string} postId - Facebook group post ID
   * @param {string} accessToken - Facebook access token (optional if configured in backend)
   * @returns {Promise<Array>} Array of analyzed comments with group info
   */
  async analyzeGroupPostComments(postId, accessToken = null) {
    const params = accessToken ? { accessToken } : {}
    return await apiClient.get(`/group-post/${postId}/analyze`, { params })
  },

  /**
   * Check if a specific user shared the post
   * @param {string} userId - Facebook user ID
   * @param {string} postUrl - URL of the original post
   * @param {string} accessToken - Facebook access token (optional if configured in backend)
   * @returns {Promise<boolean>} Whether user shared the post
   */
  async checkUserShared(userId, postUrl, accessToken = null) {
    const params = { postUrl, ...(accessToken && { accessToken }) }
    return await apiClient.get(`/user/${userId}/check-share`, { params })
  },

  /**
   * Get detailed share analysis for a user
   * @param {string} userId - Facebook user ID
   * @param {string} postUrl - URL of the original post
   * @param {string} accessToken - Facebook access token (optional if configured in backend)
   * @returns {Promise<Object>} Detailed share analysis
   */
  async analyzeUserShare(userId, postUrl, accessToken = null) {
    const params = { postUrl, ...(accessToken && { accessToken }) }
    return await apiClient.get(`/user/${userId}/share-analysis`, { params })
  },

  /**
   * Get user group information
   * @param {string} groupId - Facebook group ID
   * @param {string} userId - Facebook user ID
   * @param {string} accessToken - Facebook access token (optional if configured in backend)
   * @returns {Promise<Object>} User group info
   */
  async getUserGroupInfo(groupId, userId, accessToken = null) {
    const params = accessToken ? { accessToken } : {}
    return await apiClient.get(`/group/${groupId}/member/${userId}`, { params })
  },

  /**
   * Get API configuration
   * @returns {Promise<Object>} API configuration
   */
  async getConfig() {
    return await apiClient.get('/config')
  }
,
  /**
   * Scrape comments from a public Facebook post URL (supports public group posts)
   * @param {string} postUrl - Public Facebook post URL
   * @param {boolean} checkShare - Whether to try detect if commenters shared the post
   * @returns {Promise<Array>} Array of scraped comments
   */
  async scrapeComments(postUrl, checkShare = false) {
    return await apiClient.post('/scrape-comments', { postUrl, checkShare })
  }
}