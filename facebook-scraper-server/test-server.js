const http = require('http');
const fs = require('fs');
const path = require('path');

const server = http.createServer((req, res) => {
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
    res.setHeader('Access-Control-Allow-Headers', 'Content-Type');
    
    if (req.method === 'OPTIONS') {
        res.writeHead(200);
        res.end();
        return;
    }
    
    // Serve HTML file
    if (req.url === '/' || req.url === '/index.html') {
        fs.readFile(path.join(__dirname, 'test.html'), (err, data) => {
            if (err) {
                res.writeHead(500);
                res.end('Error loading page');
                return;
            }
            res.setHeader('Content-Type', 'text/html');
            res.writeHead(200);
            res.end(data);
        });
        return;
    }
    
    if (req.url === '/api/health' && req.method === 'GET') {
        res.setHeader('Content-Type', 'application/json');
        res.writeHead(200);
        res.end(JSON.stringify({
            status: 'OK',
            timestamp: new Date().toISOString(),
            message: 'Test server is running'
        }));
        return;
    }
    
    if (req.url === '/api/scrape' && req.method === 'POST') {
        res.setHeader('Content-Type', 'application/json');
        let body = '';
        req.on('data', chunk => {
            body += chunk.toString();
        });
        req.on('end', () => {
            try {
                const data = JSON.parse(body);
                console.log('Received scrape request:', data);
                
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
                    }
                ];
                
                res.writeHead(200);
                res.end(JSON.stringify({
                    success: true,
                    postUrl: data.postUrl || 'test-url',
                    totalComments: mockComments.length,
                    comments: mockComments
                }));
            } catch (error) {
                res.writeHead(400);
                res.end(JSON.stringify({ error: 'Invalid JSON' }));
            }
        });
        return;
    }
    
    res.setHeader('Content-Type', 'application/json');
    res.writeHead(404);
    res.end(JSON.stringify({ error: 'Not found' }));
});

const PORT = 3000;
server.listen(PORT, () => {
    console.log(`🚀 Test server running on port ${PORT}`);
    console.log(`📱 Health check: http://localhost:${PORT}/api/health`);
});

process.on('SIGINT', () => {
    console.log('\n🛑 Shutting down test server...');
    server.close(() => {
        process.exit(0);
    });
});