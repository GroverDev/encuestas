<script setup lang="ts">
import { cn } from '@/utils/cn'

const props = defineProps<{ class?: string; size?: 'sm' | 'md' | 'lg' }>()
const sizes = { sm: 'h-4 w-4', md: 'h-6 w-6', lg: 'h-8 w-8' }
</script>

<template>
  <span
    role="status"
    :class="cn('animate-spin rounded-full border-2 border-current border-t-transparent text-primary', sizes[size ?? 'md'], props.class)"
  />
</template>
