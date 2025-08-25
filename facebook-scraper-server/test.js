const fetch = require('node-fetch');

const BASE_URL = 'http://localhost:3000';

async function testAPI() {
    console.log('🧪 Testing Facebook Scraper API...\n');

    // Test health endpoint
    try {
        console.log('1. Testing health endpoint...');
        const healthResponse = await fetch(`${BASE_URL}/api/health`);
        const healthData = await healthResponse.json();
        console.log('✅ Health check:', healthData);
    } catch (error) {
        console.log('❌ Health check failed:', error.message);
    }

    // Test scrape endpoint with sample URL
    try {
        console.log('\n2. Testing scrape endpoint...');
        const sampleUrl = 'https://www.facebook.com/permalink.php?story_fbid=123456789&id=987654321';
        
        const scrapeResponse = await fetch(`${BASE_URL}/api/scrape`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ postUrl: sampleUrl })
        });
        
        const scrapeData = await scrapeResponse.json();
        console.log('✅ Scrape response:', scrapeData);
    } catch (error) {
        console.log('❌ Scrape test failed:', error.message);
    }

    // Test check-share endpoint
    try {
        console.log('\n3. Testing check-share endpoint...');
        const shareResponse = await fetch(`${BASE_URL}/api/check-share`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ 
                postUrl: 'https://www.facebook.com/permalink.php?story_fbid=123456789&id=987654321',
                userProfile: 'https://www.facebook.com/user'
            })
        });
        
        const shareData = await shareResponse.json();
        console.log('✅ Check share response:', shareData);
    } catch (error) {
        console.log('❌ Check share test failed:', error.message);
    }

    console.log('\n🎉 API testing completed!');
}

// Run tests if this file is executed directly
if (require.main === module) {
    testAPI().catch(console.error);
}

module.exports = { testAPI };