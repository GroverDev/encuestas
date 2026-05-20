import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import http from '@/utils/http'
import type { CuentaUsuario, LoginResponse, Respuesta } from '@/types/api'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const usuario = ref<CuentaUsuario | null>(null)

  const isAuthenticated = computed(() => !!token.value)

  async function login(correo: string, contrasena: string) {
    const { data } = await http.post<Respuesta<LoginResponse>>('/auth/login', { correo, contrasena })
    if (data.ok) {
      token.value = data.datos.token
      usuario.value = data.datos.usuario
      localStorage.setItem('token', data.datos.token)
    }
    return data
  }

  function logout() {
    token.value = null
    usuario.value = null
    localStorage.removeItem('token')
  }

  return { token, usuario, isAuthenticated, login, logout }
})
