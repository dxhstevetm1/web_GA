const express = require('express');
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

// Simple test endpoint
app.get('/api/health', (req, res) => {
    res.json({ 
        status: 'OK', 
        timestamp: new Date().toISOString(),
        message: 'Facebook Scraper Server is running'
    });
});

// Test scrape endpoint (without actual scraping)
app.post('/api/scrape', async (req, res) => {
    try {
        const { postUrl } = req.body;
        
        if (!postUrl) {
            return res.status(400).json({ error: 'Post URL is required' });
        }
        
        if (!postUrl.includes('facebook.com')) {
            return res.status(400).json({ error: 'Invalid Facebook URL' });
        }
        
        // Mock response for testing
        const mockComments = [
            {
                id: 'comment1',
                text: 'Đây là comment test 1',
                userName: 'Nguyễn Văn A',
                userProfile: 'https://facebook.com/user1',
                timestamp: '2 giờ trước',
                createdAt: new Date().toISOString()
            },
            {
                id: 'comment2',
                text: 'Comment test 2 với nội dung dài hơn',
                userName: 'Trần Thị B',
                userProfile: 'https://facebook.com/user2',
                timestamp: '1 giờ trước',
                createdAt: new Date().toISOString()
            },
            {
                id: 'comment3',
                text: 'Comment thứ 3 để test',
                userName: 'Lê Văn C',
                userProfile: 'https://facebook.com/user3',
                timestamp: '30 phút trước',
                createdAt: new Date().toISOString()
            }
        ];
        
        res.json({
            success: true,
            postUrl: postUrl,
            totalComments: mockComments.length,
            comments: mockComments
        });
        
    } catch (error) {
        console.error('API Error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

// Test check-share endpoint
app.post('/api/check-share', async (req, res) => {
    try {
        const { postUrl, userProfile } = req.body;
        
        if (!postUrl || !userProfile) {
            return res.status(400).json({ error: 'Post URL and user profile are required' });
        }
        
        // Mock response - randomly return true/false
        const hasShared = Math.random() > 0.5;
        
        res.json({ hasShared });
        
    } catch (error) {
        console.error('Check share error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

// Serve the main page
app.get('/', (req, res) => {
    res.sendFile(__dirname + '/public/index.html');
});

// Start server
app.listen(PORT, () => {
    console.log(`🚀 Facebook Scraper Server running on port ${PORT}`);
    console.log(`📱 Open http://localhost:${PORT} to use the application`);
    console.log(`🔧 API Health check: http://localhost:${PORT}/api/health`);
});

// Graceful shutdown
process.on('SIGINT', () => {
    console.log('\n🛑 Shutting down server...');
    process.exit(0);
});