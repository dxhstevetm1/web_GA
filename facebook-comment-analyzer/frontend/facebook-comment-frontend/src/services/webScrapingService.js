import axios from 'axios'

const API_BASE_URL = '/api/webscraping'

// Create axios instance for web scraping API
const scrapingClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 120000, // 2 minutes for scraping operations
  headers: {
    'Content-Type': 'application/json'
  }
})

// Response interceptor to handle errors
scrapingClient.interceptors.response.use(
  (response) => {
    return response.data
  },
  (error) => {
    if (error.response) {
      // Server responded with error status
      const message = error.response.data?.Error || error.response.data?.error || 'An error occurred'
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

export const webScrapingService = {
  /**
   * Scrape all comments from a Facebook post URL
   * @param {string} postUrl - Facebook post URL
   * @param {Object} options - Scraping options
   * @returns {Promise<Object>} Scraped post data with comments
   */
  async scrapePostComments(postUrl, options = {}) {
    const request = {
      postUrl,
      maxComments: options.maxComments || 1000,
      sortOrder: options.sortOrder || 'OldestFirst',
      loadReplies: options.loadReplies || false,
      loadReactions: options.loadReactions || false,
      useHeadlessBrowser: options.useHeadlessBrowser !== false,
      scrollDelayMs: options.scrollDelayMs || 2000,
      maxScrollAttempts: options.maxScrollAttempts || 50
    }
    
    return await scrapingClient.post('/scrape-post-comments', request)
  },

  /**
   * Get basic post information from URL
   * @param {string} postUrl - Facebook post URL
   * @returns {Promise<Object>} Post information
   */
  async getPostInfo(postUrl) {
    return await scrapingClient.post('/get-post-info', { postUrl })
  },

  /**
   * Validate Facebook post URL
   * @param {string} url - URL to validate
   * @returns {Promise<Object>} Validation result with URL info
   */
  async validateUrl(url) {
    return await scrapingClient.post('/validate-url', { url })
  },

  /**
   * Analyze comments with advanced filtering
   * @param {string} postUrl - Facebook post URL
   * @param {Object} options - Analysis options and filters
   * @returns {Promise<Object>} Analyzed comments result
   */
  async analyzeComments(postUrl, options = {}) {
    const request = {
      postUrl,
      maxComments: options.maxComments || 1000,
      sortOrder: options.sortOrder || 'OldestFirst',
      loadReplies: options.loadReplies || false,
      loadReactions: options.loadReactions || false,
      filters: options.filters || {}
    }
    
    return await scrapingClient.post('/analyze-comments', request)
  },

  /**
   * Get list of users who shared the post
   * @param {string} postUrl - Facebook post URL
   * @param {Object} options - Options
   * @returns {Promise<Object>} List of sharers
   */
  async getSharers(postUrl, options = {}) {
    const request = {
      postUrl,
      maxComments: options.maxComments || 1000
    }
    
    return await scrapingClient.post('/get-sharers', request)
  }
}

// Export sort order constants
export const SortOrder = {
  OLDEST_FIRST: 'OldestFirst',
  NEWEST_FIRST: 'NewestFirst',
  MOST_RELEVANT: 'MostRelevant'
}

// Export default options
export const defaultScrapingOptions = {
  maxComments: 1000,
  sortOrder: SortOrder.OLDEST_FIRST,
  loadReplies: false,
  loadReactions: false,
  useHeadlessBrowser: true,
  scrollDelayMs: 2000,
  maxScrollAttempts: 50
}