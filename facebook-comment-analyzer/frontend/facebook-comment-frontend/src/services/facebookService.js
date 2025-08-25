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
   * @param {string} accessToken - Facebook access token
   * @returns {Promise<Object>} Post information
   */
  async getPost(postId, accessToken) {
    return await apiClient.get(`/post/${postId}`, {
      params: { accessToken }
    })
  },

  /**
   * Get all comments for a Facebook post
   * @param {string} postId - Facebook post ID
   * @param {string} accessToken - Facebook access token
   * @returns {Promise<Array>} Array of comments
   */
  async getComments(postId, accessToken) {
    return await apiClient.get(`/post/${postId}/comments`, {
      params: { accessToken }
    })
  },

  /**
   * Analyze comments and check if users shared the post
   * @param {string} postId - Facebook post ID
   * @param {string} accessToken - Facebook access token
   * @returns {Promise<Array>} Array of analyzed comments
   */
  async analyzeComments(postId, accessToken) {
    return await apiClient.get(`/post/${postId}/analyze`, {
      params: { accessToken }
    })
  },

  /**
   * Check if a specific user shared the post
   * @param {string} userId - Facebook user ID
   * @param {string} postUrl - URL of the original post
   * @param {string} accessToken - Facebook access token
   * @returns {Promise<boolean>} Whether user shared the post
   */
  async checkUserShared(userId, postUrl, accessToken) {
    return await apiClient.get(`/user/${userId}/check-share`, {
      params: { postUrl, accessToken }
    })
  }
}