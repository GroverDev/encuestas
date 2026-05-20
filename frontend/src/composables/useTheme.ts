import { ref, onMounted } from 'vue'

const isDark = ref(false)

export function useTheme() {
  function apply(dark: boolean) {
    isDark.value = dark
    document.documentElement.classList.toggle('dark', dark)
    localStorage.setItem('theme', dark ? 'dark' : 'light')
  }

  function toggle() { apply(!isDark.value) }

  onMounted(() => {
    const stored = localStorage.getItem('theme')
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
    apply(stored ? stored === 'dark' : prefersDark)
  })

  return { isDark, toggle }
}
