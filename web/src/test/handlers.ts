import { http, HttpResponse } from 'msw'

const API_URL = 'http://localhost:8000/api'

export const handlers = [
  // Clients
  http.get(`${API_URL}/clients`, () => {
    return HttpResponse.json([
      { id: '1', name: 'Mock Client 1', email: 'v1@example.com' },
      { id: '2', name: 'Mock Client 2', email: 'v2@example.com' },
    ])
  }),

  http.post(`${API_URL}/clients`, async ({ request }) => {
    const body = await request.json() as any
    return HttpResponse.json({ id: '3', name: body.name }, { status: 201 })
  }),

  // Leads
  http.get(`${API_URL}/leads`, () => {
    return HttpResponse.json([
      { id: '1', title: 'Mock Lead 1', status: 'New' },
    ])
  }),

  // Auth
  http.post(`${API_URL}/auth/login`, async () => {
    return HttpResponse.json({
      user: { id: '1', email: 'admin@example.com', fullName: 'Admin User' },
      accessToken: 'mock-access-token'
    })
  }),

  // Projects
  http.get(`${API_URL}/projects`, () => {
    return HttpResponse.json([
      { id: '1', name: 'Mock Project 1' }
    ])
  }),

  // Offers
  http.get(`${API_URL}/offers`, () => {
    return HttpResponse.json([
      { id: '1', title: 'Mock Offer 1', status: 'Draft' }
    ])
  })
]
