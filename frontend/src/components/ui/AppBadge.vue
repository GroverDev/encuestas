<script setup lang="ts">
import { cn } from '@/utils/cn'
import { cva } from 'class-variance-authority'

const variants = cva(
  'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium transition-colors',
  {
    variants: {
      variant: {
        default: 'bg-primary/10 text-primary',
        secondary: 'bg-secondary text-secondary-foreground',
        destructive: 'bg-destructive/10 text-destructive',
        success: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400',
        warning: 'bg-amber-500/10 text-amber-600 dark:text-amber-400',
        outline: 'border border-border text-foreground',
      },
    },
    defaultVariants: { variant: 'default' },
  },
)

const props = withDefaults(
  defineProps<{
    variant?: 'default' | 'secondary' | 'destructive' | 'success' | 'warning' | 'outline'
    class?: string
  }>(),
  { variant: 'default' },
)
</script>

<template>
  <span :class="cn(variants({ variant }), props.class)"><slot /></span>
</template>
