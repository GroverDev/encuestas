<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import useVuelidate from '@vuelidate/core'
import { required, email, minLength } from '@vuelidate/validators'
import { useAuthStore } from '@/stores/auth'
import { useToast } from '@/composables/useToast'
import { useTheme } from '@/composables/useTheme'
import AppButton from '@/components/ui/AppButton.vue'
import AppInput from '@/components/ui/AppInput.vue'
import { Sun, Moon } from 'lucide-vue-next'

const router = useRouter()
const auth = useAuthStore()
const { toast } = useToast()
const { isDark, toggle } = useTheme()

const form = reactive({ correo: '', contrasena: '' })
const rules = {
  correo: { required, email },
  contrasena: { required, minLength: minLength(6) },
}
const v$ = useVuelidate(rules, form)

const loading = ref(false)

async function onSubmit() {
  const valid = await v$.value.$validate()
  if (!valid) return
  loading.value = true
  try {
    const resp = await auth.login(form.correo, form.contrasena)
    if (resp.ok) {
      toast.success('Bienvenido')
      router.push('/')
    } else {
      toast.error('Credenciales inválidas')
    }
  } catch {
    toast.error('Error al conectar con el servidor')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-background p-4">
    <!-- Theme toggle -->
    <button
      class="fixed top-4 right-4 rounded-md p-2 text-muted-foreground hover:bg-accent hover:text-foreground transition-colors"
      @click="toggle"
    >
      <Sun v-if="isDark" class="h-5 w-5" />
      <Moon v-else class="h-5 w-5" />
    </button>

    <div class="w-full max-w-sm space-y-8">
      <!-- Header -->
      <div class="text-center space-y-2">
        <div class="mx-auto h-12 w-12 rounded-xl bg-primary flex items-center justify-center text-white font-bold text-xl">
          E
        </div>
        <h1 class="text-2xl font-bold">Iniciar sesión</h1>
        <p class="text-sm text-muted-foreground">Motor de Encuestas Enterprise</p>
      </div>

      <!-- Form -->
      <div class="rounded-xl border border-border bg-card p-6 shadow-sm space-y-4">
        <form class="space-y-4" @submit.prevent="onSubmit">
          <AppInput
            v-model="form.correo"
            label="Correo electrónico"
            type="email"
            placeholder="usuario@empresa.com"
            :error="v$.correo.$error ? 'Ingresa un correo válido' : undefined"
            @blur="v$.correo.$touch"
          />
          <AppInput
            v-model="form.contrasena"
            label="Contraseña"
            type="password"
            placeholder="••••••••"
            :error="v$.contrasena.$error ? 'Mínimo 6 caracteres' : undefined"
            @blur="v$.contrasena.$touch"
          />
          <AppButton type="submit" class="w-full" :loading="loading">
            Ingresar
          </AppButton>
        </form>
      </div>
    </div>
  </div>
</template>
