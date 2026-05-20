<script setup lang="ts">
import { cn } from '@/utils/cn'

const props = defineProps<{
  class?: string
  hover?: boolean
}>()
</script>

<template>
  <div :class="cn('rounded-xl border border-border bg-card text-card-foreground shadow-sm', hover && 'card-hover', props.class)">
    <slot />
  </div>
</template>
