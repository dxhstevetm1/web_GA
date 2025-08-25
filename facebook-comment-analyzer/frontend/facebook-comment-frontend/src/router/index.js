import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import WebScrapingView from '../views/WebScrapingView.vue'

const routes = [
  {
    path: '/',
    name: 'home',
    component: HomeView
  },
  {
    path: '/scraping',
    name: 'webscraping',
    component: WebScrapingView
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router