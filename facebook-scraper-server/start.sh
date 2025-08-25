#!/bin/bash

echo "🚀 Facebook Comment Scraper - Startup Script"
echo "=============================================="

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "❌ Node.js is not installed. Please install Node.js first."
    exit 1
fi

# Check if npm is installed
if ! command -v npm &> /dev/null; then
    echo "❌ npm is not installed. Please install npm first."
    exit 1
fi

echo "✅ Node.js and npm are installed"

# Install dependencies if node_modules doesn't exist
if [ ! -d "node_modules" ]; then
    echo "📦 Installing dependencies..."
    npm install
    if [ $? -ne 0 ]; then
        echo "❌ Failed to install dependencies"
        exit 1
    fi
    echo "✅ Dependencies installed successfully"
else
    echo "✅ Dependencies already installed"
fi

# Check if .env file exists
if [ ! -f ".env" ]; then
    echo "📝 Creating .env file from .env.example..."
    cp .env.example .env
    echo "✅ .env file created"
fi

# Choose server mode
echo ""
echo "Choose server mode:"
echo "1) Production server (with Puppeteer scraping)"
echo "2) Test server (mock data)"
echo "3) Development server (with nodemon)"
read -p "Enter your choice (1-3): " choice

case $choice in
    1)
        echo "🚀 Starting production server..."
        node server.js
        ;;
    2)
        echo "🧪 Starting test server..."
        node test-server.js
        ;;
    3)
        echo "🔧 Starting development server..."
        npm run dev
        ;;
    *)
        echo "❌ Invalid choice. Exiting..."
        exit 1
        ;;
esac