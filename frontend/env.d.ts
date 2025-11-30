/// <reference types="vite/client" />

import type { DefineComponent } from 'vue'

declare module 'vue' {
  export interface GlobalComponents {
    HomeIcon: typeof import('lucide-vue-next')['Home']
    MessageSquareIcon: typeof import('lucide-vue-next')['MessageSquare']
    BarChart3Icon: typeof import('lucide-vue-next')['BarChart3']
    InfoIcon: typeof import('lucide-vue-next')['Info']
    MoonIcon: typeof import('lucide-vue-next')['Moon']
    SunIcon: typeof import('lucide-vue-next')['Sun']
    SendIcon: typeof import('lucide-vue-next')['Send']
    RotateCcwIcon: typeof import('lucide-vue-next')['RotateCcw']
    CheckCircleIcon: typeof import('lucide-vue-next')['CheckCircle']
    AlertCircleIcon: typeof import('lucide-vue-next')['AlertCircle']
    TrendingUpIcon: typeof import('lucide-vue-next')['TrendingUp']
    UsersIcon: typeof import('lucide-vue-next')['Users']
    StarIcon: typeof import('lucide-vue-next')['Star']
    ArrowRightIcon: typeof import('lucide-vue-next')['ArrowRight']
  }
}
