const express = require('express');
const puppeteer = require('puppeteer');
const cors = require('cors');
const helmet = require('helmet');
require('dotenv').config();

const app = express();
const PORT = process.env.PORT || 3000;

// Middleware
app.use(helmet());
app.use(cors());
app.use(express.json());
app.use(express.static('public'));

// Store browser instance
let browser = null;

// Initialize browser
async function initBrowser() {
    if (!browser) {
        try {
            browser = await puppeteer.launch({
                headless: process.env.NODE_ENV === 'production' ? 'new' : false,
                args: [
                    '--no-sandbox',
                    '--disable-setuid-sandbox',
                    '--disable-dev-shm-usage',
                    '--disable-accelerated-2d-canvas',
                    '--no-first-run',
                    '--no-zygote',
                    '--disable-gpu',
                    '--disable-background-timer-throttling',
                    '--disable-backgrounding-occluded-windows',
                    '--disable-renderer-backgrounding',
                    '--disable-web-security',
                    '--disable-features=VizDisplayCompositor'
                ],
                executablePath: process.env.PUPPETEER_EXECUTABLE_PATH || undefined
            });
            console.log('Browser initialized successfully');
        } catch (error) {
            console.error('Failed to initialize browser:', error);
            throw error;
        }
    }
    return browser;
}

// Parse Facebook post URL to get post ID
function parseFacebookUrl(url) {
    const patterns = [
        /facebook\.com\/.*?\/posts\/(\d+)/,
        /facebook\.com\/.*?\/permalink\/(\d+)/,
        /facebook\.com\/.*?\/activity\/(\d+)/,
        /facebook\.com\/.*?\/videos\/(\d+)/,
        /facebook\.com\/.*?\/photos\/(\d+)/,
        /facebook\.com\/.*?\/groups\/.*?\/posts\/(\d+)/,
        /facebook\.com\/.*?\/groups\/.*?\/permalink\/(\d+)/
    ];
    
    for (const pattern of patterns) {
        const match = url.match(pattern);
        if (match) {
            return match[1];
        }
    }
    return null;
}

// Scrape Facebook post comments
async function scrapeFacebookComments(postUrl) {
    const browser = await initBrowser();
    const page = await browser.newPage();
    
    try {
        // Set user agent to avoid detection
        await page.setUserAgent('Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36');
        
        // Set viewport
        await page.setViewport({ width: 1920, height: 1080 });
        
        // Set extra headers
        await page.setExtraHTTPHeaders({
            'Accept-Language': 'en-US,en;q=0.9,vi;q=0.8',
            'Accept-Encoding': 'gzip, deflate, br',
            'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
            'Cache-Control': 'no-cache',
            'Pragma': 'no-cache'
        });
        
        console.log('Navigating to Facebook post...');
        await page.goto(postUrl, { waitUntil: 'networkidle2', timeout: 30000 });
        
        // Wait for content to load
        await page.waitForTimeout(3000);
        
        // Check if page is accessible
        const isAccessible = await page.evaluate(() => {
            const errorElements = document.querySelectorAll('[data-testid="error_message"], .error_message');
            return errorElements.length === 0;
        });
        
        if (!isAccessible) {
            return {
                success: false,
                error: 'Bài viết không thể truy cập hoặc không tồn tại'
            };
        }
        
        // Scroll to load more comments
        console.log('Scrolling to load comments...');
        await autoScroll(page);
        
        // Extract comments with multiple selectors for better compatibility
        const comments = await page.evaluate(() => {
            const comments = [];
            
            // Multiple selectors for different Facebook layouts
            const selectors = [
                '[data-testid="comment"]',
                '[data-testid="UFI2Comment/root"]',
                '.UFIComment',
                '[role="article"]'
            ];
            
            let commentElements = [];
            for (const selector of selectors) {
                commentElements = document.querySelectorAll(selector);
                if (commentElements.length > 0) break;
            }
            
            commentElements.forEach((commentEl) => {
                try {
                    // Get comment text with multiple selectors
                    const textSelectors = [
                        '[data-testid="comment_message"]',
                        '.UFICommentBody',
                        '[data-testid="UFI2Comment/body"]',
                        '.comment_body'
                    ];
                    
                    let text = '';
                    for (const textSelector of textSelectors) {
                        const textEl = commentEl.querySelector(textSelector);
                        if (textEl) {
                            text = textEl.textContent.trim();
                            break;
                        }
                    }
                    
                    // Get user name and profile link
                    const userSelectors = [
                        'a[role="link"]',
                        '.UFICommentActorName',
                        '[data-testid="UFI2Comment/actor"] a',
                        '.comment_actor a'
                    ];
                    
                    let userName = '';
                    let userProfile = '';
                    for (const userSelector of userSelectors) {
                        const userEl = commentEl.querySelector(userSelector);
                        if (userEl) {
                            userName = userEl.textContent.trim();
                            userProfile = userEl.href;
                            break;
                        }
                    }
                    
                    // Get timestamp
                    const timeSelectors = [
                        'a[role="link"][href*="/permalink/"]',
                        '.UFICommentTimestamp',
                        '[data-testid="UFI2Comment/timestamp"]',
                        '.comment_timestamp'
                    ];
                    
                    let timestamp = '';
                    for (const timeSelector of timeSelectors) {
                        const timeEl = commentEl.querySelector(timeSelector);
                        if (timeEl) {
                            timestamp = timeEl.textContent.trim();
                            break;
                        }
                    }
                    
                    // Get comment ID
                    const commentId = commentEl.getAttribute('data-testid') || 
                                    commentEl.getAttribute('id') || 
                                    Math.random().toString(36).substr(2, 9);
                    
                    if (text && userName) {
                        comments.push({
                            id: commentId,
                            text: text,
                            userName: userName,
                            userProfile: userProfile,
                            timestamp: timestamp,
                            createdAt: new Date().toISOString()
                        });
                    }
                } catch (error) {
                    console.error('Error parsing comment:', error);
                }
            });
            
            return comments;
        });
        
        // Sort comments by timestamp (oldest first)
        const sortedComments = sortCommentsByTime(comments);
        
        return {
            success: true,
            postUrl: postUrl,
            totalComments: sortedComments.length,
            comments: sortedComments
        };
        
    } catch (error) {
        console.error('Error scraping Facebook:', error);
        return {
            success: false,
            error: error.message
        };
    } finally {
        await page.close();
    }
}

// Auto scroll to load more comments
async function autoScroll(page) {
    await page.evaluate(async () => {
        await new Promise((resolve) => {
            let totalHeight = 0;
            const distance = 100;
            const timer = setInterval(() => {
                const scrollHeight = document.body.scrollHeight;
                window.scrollBy(0, distance);
                totalHeight += distance;
                
                if (totalHeight >= scrollHeight) {
                    clearInterval(timer);
                    resolve();
                }
            }, 100);
        });
    });
    
    // Wait a bit more for content to load
    await page.waitForTimeout(2000);
}

// Sort comments by time (oldest first)
function sortCommentsByTime(comments) {
    return comments.sort((a, b) => {
        // Parse relative time to actual time
        const timeA = parseRelativeTime(a.timestamp);
        const timeB = parseRelativeTime(b.timestamp);
        return timeA - timeB;
    });
}

// Parse relative time to actual time
function parseRelativeTime(relativeTime) {
    if (!relativeTime) return Date.now();
    
    const now = new Date();
    const lowerTime = relativeTime.toLowerCase();
    
    if (lowerTime.includes('giây') || lowerTime.includes('second')) {
        return now.getTime() - (parseInt(lowerTime.match(/\d+/)?.[0] || 0) * 1000);
    }
    if (lowerTime.includes('phút') || lowerTime.includes('minute')) {
        return now.getTime() - (parseInt(lowerTime.match(/\d+/)?.[0] || 0) * 60 * 1000);
    }
    if (lowerTime.includes('giờ') || lowerTime.includes('hour')) {
        return now.getTime() - (parseInt(lowerTime.match(/\d+/)?.[0] || 0) * 60 * 60 * 1000);
    }
    if (lowerTime.includes('ngày') || lowerTime.includes('day')) {
        return now.getTime() - (parseInt(lowerTime.match(/\d+/)?.[0] || 0) * 24 * 60 * 60 * 1000);
    }
    if (lowerTime.includes('tuần') || lowerTime.includes('week')) {
        return now.getTime() - (parseInt(lowerTime.match(/\d+/)?.[0] || 0) * 7 * 24 * 60 * 60 * 1000);
    }
    if (lowerTime.includes('tháng') || lowerTime.includes('month')) {
        return now.getTime() - (parseInt(lowerTime.match(/\d+/)?.[0] || 0) * 30 * 24 * 60 * 60 * 1000);
    }
    if (lowerTime.includes('năm') || lowerTime.includes('year')) {
        return now.getTime() - (parseInt(lowerTime.match(/\d+/)?.[0] || 0) * 365 * 24 * 60 * 60 * 1000);
    }
    
    return now.getTime();
}

// Check if user has shared the post
async function checkUserShare(postUrl, userProfile) {
    const browser = await initBrowser();
    const page = await browser.newPage();
    
    try {
        await page.setUserAgent('Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36');
        
        // Set extra headers
        await page.setExtraHTTPHeaders({
            'Accept-Language': 'en-US,en;q=0.9,vi;q=0.8',
            'Accept-Encoding': 'gzip, deflate, br',
            'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
            'Cache-Control': 'no-cache',
            'Pragma': 'no-cache'
        });
        
        // Navigate to user profile
        await page.goto(userProfile, { waitUntil: 'networkidle2', timeout: 30000 });
        await page.waitForTimeout(3000);
        
        // Extract post ID from URL
        const postId = extractPostId(postUrl);
        
        // Check if the post appears in user's timeline with multiple methods
        const hasShared = await page.evaluate((postId) => {
            // Method 1: Check for post links
            const links = Array.from(document.querySelectorAll('a[href]'));
            const hasPostLink = links.some(link => {
                const href = link.href;
                return href.includes(`/posts/${postId}`) || 
                       href.includes(`/permalink/${postId}`) ||
                       href.includes(`/activity/${postId}`);
            });
            
            if (hasPostLink) return true;
            
            // Method 2: Check for shared content indicators
            const shareIndicators = [
                '[data-testid="shared_post"]',
                '.shared_post',
                '[data-testid="UFI2Share/root"]',
                '.UFIShare'
            ];
            
            for (const indicator of shareIndicators) {
                const elements = document.querySelectorAll(indicator);
                if (elements.length > 0) {
                    // Check if any shared post contains the target post ID
                    for (const element of elements) {
                        const elementLinks = element.querySelectorAll('a[href]');
                        for (const link of elementLinks) {
                            if (link.href.includes(postId)) {
                                return true;
                            }
                        }
                    }
                }
            }
            
            // Method 3: Check for "shared" text in posts
            const posts = document.querySelectorAll('[data-testid="post_message"], .post_message, [role="article"]');
            for (const post of posts) {
                const text = post.textContent.toLowerCase();
                if (text.includes('shared') || text.includes('chia sẻ')) {
                    const postLinks = post.querySelectorAll('a[href]');
                    for (const link of postLinks) {
                        if (link.href.includes(postId)) {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }, postId);
        
        return hasShared;
        
    } catch (error) {
        console.error('Error checking user share:', error);
        return false;
    } finally {
        await page.close();
    }
}

// Extract post ID from Facebook URL
function extractPostId(url) {
    const patterns = [
        /facebook\.com\/.*?\/posts\/(\d+)/,
        /facebook\.com\/.*?\/permalink\/(\d+)/,
        /facebook\.com\/.*?\/activity\/(\d+)/,
        /facebook\.com\/.*?\/videos\/(\d+)/,
        /facebook\.com\/.*?\/photos\/(\d+)/,
        /facebook\.com\/.*?\/groups\/.*?\/posts\/(\d+)/,
        /facebook\.com\/.*?\/groups\/.*?\/permalink\/(\d+)/
    ];
    
    for (const pattern of patterns) {
        const match = url.match(pattern);
        if (match) {
            return match[1];
        }
    }
    return '';
}

// API Routes
app.get('/', (req, res) => {
    res.sendFile(__dirname + '/public/index.html');
});

app.post('/api/scrape', async (req, res) => {
    try {
        const { postUrl } = req.body;
        
        if (!postUrl) {
            return res.status(400).json({ error: 'Post URL is required' });
        }
        
        if (!postUrl.includes('facebook.com')) {
            return res.status(400).json({ error: 'Invalid Facebook URL' });
        }
        
        console.log('Starting scrape for:', postUrl);
        const result = await scrapeFacebookComments(postUrl);
        
        res.json(result);
        
    } catch (error) {
        console.error('API Error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

app.post('/api/check-share', async (req, res) => {
    try {
        const { postUrl, userProfile } = req.body;
        
        if (!postUrl || !userProfile) {
            return res.status(400).json({ error: 'Post URL and user profile are required' });
        }
        
        const hasShared = await checkUserShare(postUrl, userProfile);
        
        res.json({ hasShared });
        
    } catch (error) {
        console.error('Check share error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

// Health check
app.get('/api/health', (req, res) => {
    res.json({ status: 'OK', timestamp: new Date().toISOString() });
});

// Start server
app.listen(PORT, () => {
    console.log(`Facebook Scraper Server running on port ${PORT}`);
    console.log(`Open http://localhost:${PORT} to use the application`);
});

// Graceful shutdown
process.on('SIGINT', async () => {
    console.log('Shutting down server...');
    if (browser) {
        await browser.close();
    }
    process.exit(0);
});