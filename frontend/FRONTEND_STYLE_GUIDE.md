# JackLimited Frontend Design System & Style Guide

## Overview

This document outlines the comprehensive design system and styling guidelines for the JackLimited Vue.js application. It serves as the authoritative reference for maintaining visual consistency across all components and pages.

## Design Philosophy

- **Modern & Professional**: Clean, contemporary design that feels polished and trustworthy
- **Responsive First**: Mobile-first approach with adaptive layouts for all screen sizes
- **Accessible**: High contrast ratios and semantic color usage
- **Consistent**: Unified design tokens and component patterns
- **Maintainable**: CSS variables and systematic organization

## Color Palette

### Primary Colors
```css
--color-primary: #6366f1;           /* Indigo - main brand color */
--color-primary-hover: #4f46e5;     /* Darker indigo for hover states */
--color-primary-light: #a5b4fc;     /* Light indigo for accents */
```

### Secondary Colors
```css
--color-secondary: #f59e0b;         /* Amber - secondary actions */
--color-secondary-hover: #d97706;   /* Darker amber for hover */
```

### Accent Colors
```css
--color-accent: #10b981;            /* Emerald - success states */
--color-accent-hover: #059669;      /* Darker emerald for hover */
```

### Semantic Colors
```css
--color-success: #10b981;           /* Success states */
--color-warning: #f59e0b;           /* Warning states */
--color-error: #ef4444;             /* Error states */
--color-info: #3b82f6;              /* Info states */
```

### Neutral Colors
```css
--color-background: #ffffff;        /* Main background */
--color-background-secondary: #f8fafc; /* Secondary backgrounds */
--color-background-tertiary: #f1f5f9;  /* Tertiary backgrounds */

--color-surface: #ffffff;           /* Card/component backgrounds */
--color-surface-hover: #f8fafc;     /* Surface hover states */

--color-text: #1e293b;              /* Primary text */
--color-text-secondary: #64748b;    /* Secondary text */
--color-text-muted: #94a3b8;        /* Muted text */
--color-text-inverse: #ffffff;      /* Text on dark backgrounds */

--color-border: #e2e8f0;            /* Default borders */
--color-border-hover: #cbd5e1;      /* Border hover states */
--color-border-focus: #6366f1;      /* Focus ring color */
```

### Shadows
```css
--color-shadow: rgba(0, 0, 0, 0.1);     /* Subtle shadows */
--color-shadow-lg: rgba(0, 0, 0, 0.15); /* Larger shadows */
```

### Gradients
```css
--gradient-primary: linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%);
--gradient-secondary: linear-gradient(135deg, #f59e0b 0%, #f97316 100%);
--gradient-accent: linear-gradient(135deg, #10b981 0%, #06b6d4 100%);
```

## Dark Mode Colors

All colors automatically adapt for dark mode using CSS media queries and class-based overrides:

```css
@media (prefers-color-scheme: dark) {
  --color-background: #0f172a;
  --color-background-secondary: #1e293b;
  --color-background-tertiary: #334155;
  --color-surface: #1e293b;
  --color-surface-hover: #334155;
  --color-text: #f1f5f9;
  --color-text-secondary: #cbd5e1;
  --color-text-muted: #94a3b8;
  --color-text-inverse: #0f172a;
  --color-border: #334155;
  --color-border-hover: #475569;
  --color-shadow: rgba(0, 0, 0, 0.3);
  --color-shadow-lg: rgba(0, 0, 0, 0.4);
}
```

## Typography

### Font Family
```css
--font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Fira Sans', 'Droid Sans', 'Helvetica Neue', sans-serif;
```

### Font Sizes (Responsive Scale)
```css
--font-size-xs: 0.75rem;    /* 12px */
--font-size-sm: 0.875rem;   /* 14px */
--font-size-base: 1rem;     /* 16px */
--font-size-lg: 1.25rem;    /* 20px */
--font-size-xl: 2rem;       /* 32px */
--font-size-2xl: 2.5rem;    /* 40px */
--font-size-3xl: 3rem;      /* 48px */
```

### Font Weights
```css
--font-weight-normal: 400;
--font-weight-medium: 500;
--font-weight-semibold: 600;
--font-weight-bold: 700;
```

### Line Heights
```css
--line-height-tight: 1.25;
--line-height-normal: 1.5;
--line-height-relaxed: 1.75;
```

## Spacing System

### Spacing Scale
```css
--spacing-xs: 0.25rem;      /* 4px */
--spacing-sm: 0.5rem;       /* 8px */
--spacing-md: 1rem;         /* 16px */
--spacing-lg: 1.5rem;       /* 24px */
--spacing-xl: 2rem;         /* 32px */
--spacing-2xl: 3rem;        /* 48px */
```

## Layout & Responsive Design

### Breakpoints
- **Mobile**: < 768px
- **Tablet**: 768px - 1023px
- **Desktop**: 1024px - 1439px
- **Large Desktop**: 1440px+

### Container Max-Widths
- **Mobile/Tablet**: Full width with padding
- **Desktop**: 1200px max-width, centered
- **Large Desktop**: 1200px max-width, centered

### Layout Patterns

#### Page Layout
```css
.content-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 var(--spacing-lg);
}

@media (max-width: 768px) {
  .content-wrapper {
    padding: 0 var(--spacing-md);
  }
}
```

#### Card Layout
```css
.card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius-lg);
  padding: var(--spacing-xl);
  box-shadow: 0 1px 3px var(--color-shadow);
  transition: box-shadow var(--transition-normal);
}

.card:hover {
  box-shadow: 0 4px 6px var(--color-shadow-lg);
}
```

## Component Patterns

### Buttons

#### Primary Button
```css
.btn-primary {
  background: var(--color-primary);
  color: var(--color-text-inverse);
  border: 1px solid var(--color-primary);
  border-radius: var(--border-radius);
  padding: var(--spacing-sm) var(--spacing-lg);
  font-weight: var(--font-weight-medium);
  transition: all var(--transition-normal);
}

.btn-primary:hover {
  background: var(--color-primary-hover);
  border-color: var(--color-primary-hover);
}
```

#### Secondary Button
```css
.btn-secondary {
  background: transparent;
  color: var(--color-text);
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius);
  padding: var(--spacing-sm) var(--spacing-lg);
  font-weight: var(--font-weight-medium);
  transition: all var(--transition-normal);
}

.btn-secondary:hover {
  background: var(--color-surface-hover);
  border-color: var(--color-border-hover);
}
```

### Form Elements

#### Input Fields
```css
.form-input {
  width: 100%;
  padding: var(--spacing-md);
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius);
  background: var(--color-surface);
  color: var(--color-text);
  font-size: var(--font-size-base);
  transition: border-color var(--transition-normal);
}

.form-input:focus {
  outline: none;
  border-color: var(--color-border-focus);
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
}
```

#### Labels
```css
.form-label {
  display: block;
  margin-bottom: var(--spacing-sm);
  font-weight: var(--font-weight-medium);
  color: var(--color-text);
}
```

### Chart Components

#### Shadow Guidelines
- **No Individual Shadows**: Chart components (NpsChart, RatingDistribution) should not have their own shadow boxes when used inside `.card` containers
- **Card Wrapper**: Use the global `.card` class to provide consistent shadow and background styling
- **Overflow Control**: Chart containers must have `overflow: hidden` to prevent chart content from exceeding card boundaries
- **Width Constraints**: Chart components should use `width: 100%` when inside cards

```css
/* ✅ Correct - Chart component inside card */
.chart-component {
  width: 100%;
  margin: 0;
  padding: 0;
  overflow: hidden;
}

.chart-container {
  position: relative;
  height: 300px;
  overflow: hidden;
}
```

### Navigation

#### Navigation Bar
```css
.navbar {
  background: var(--color-surface);
  border-bottom: 1px solid var(--color-border);
  padding: var(--spacing-md) 0;
}

.navbar-brand {
  font-weight: var(--font-weight-bold);
  font-size: var(--font-size-lg);
  color: var(--color-text);
  white-space: nowrap; /* Prevents wrapping */
}

.nav-link {
  color: var(--color-text-secondary);
  text-decoration: none;
  padding: var(--spacing-sm) var(--spacing-md);
  border-radius: var(--border-radius);
  transition: color var(--transition-normal);
}

.nav-link:hover {
  color: var(--color-text);
  background: var(--color-surface-hover);
}
```

## Icon System

### Icon Library
- **Library**: Lucide Vue Next
- **Usage**: Import specific icons as needed
- **Styling**: Use currentColor for automatic theme adaptation

### Icon Usage Guidelines

#### Navigation Icons
- Home: `HomeIcon`
- Survey/Feedback: `MessageSquareIcon`
- Analytics: `BarChart3Icon`
- About: `InfoIcon`

#### Action Icons
- Submit/Send: `SendIcon`
- Refresh/Retry: `RotateCcwIcon`
- Success: `CheckCircleIcon`
- Error/Warning: `AlertCircleIcon`

#### Theme Toggle
- Light mode: `SunIcon`
- Dark mode: `MoonIcon`

### Icon Implementation
```vue
<template>
  <HomeIcon class="icon" />
</template>

<style scoped>
.icon {
  width: 1.25rem;
  height: 1.25rem;
  color: currentColor;
}
</style>
```

## Transitions & Animations

### Transition Variables
```css
--transition-fast: 150ms ease-in-out;
--transition-normal: 250ms ease-in-out;
--transition-slow: 350ms ease-in-out;
```

### Hover Effects
- Use `var(--transition-normal)` for smooth color/background changes
- Scale transforms: `transform: scale(1.02)` for subtle lift effects
- Shadow changes for depth indication

## Responsive Design Guidelines

### Mobile First Approach
1. Start with mobile styles (default)
2. Add tablet styles at 768px+
3. Add desktop styles at 1024px+
4. Add large desktop styles at 1440px+

### Responsive Patterns

#### Responsive Spacing
```css
.element {
  padding: var(--spacing-md); /* Mobile default */
}

@media (min-width: 768px) {
  .element {
    padding: var(--spacing-lg); /* Tablet and up */
  }
}
```

#### Responsive Typography
```css
.heading {
  font-size: var(--font-size-xl); /* Mobile */
}

@media (min-width: 1024px) {
  .heading {
    font-size: var(--font-size-2xl); /* Desktop */
  }
}
```

#### Responsive Layouts
```css
.grid {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

@media (min-width: 1024px) {
  .grid {
    flex-direction: row;
  }
}
```

## Implementation Guidelines

### CSS Organization
1. **base.css**: Design tokens, global resets, body styles
2. **global.css**: Component styles, utilities, responsive rules
3. **main.css**: Import statements only
4. **Component styles**: Scoped styles for specific components

### Vue.js Best Practices
- Use CSS variables for all design tokens
- Implement responsive design with mobile-first approach
- Use semantic color names (not hex values directly)
- Leverage CSS custom properties for theme switching
- Maintain consistent spacing using the spacing scale

### File Structure
```
frontend/src/
├── assets/
│   ├── base.css          # Design tokens & global styles
│   ├── global.css        # Component patterns & utilities
│   └── main.css          # Import aggregation
├── components/
│   ├── NavigationBar.vue # Navigation component
│   ├── SurveyForm.vue    # Form component
│   └── ...
└── views/
    ├── HomeView.vue      # Page views
    ├── SurveyView.vue
    └── ...
```

## Maintenance Guidelines

### Adding New Colors
1. Define in CSS variables in `base.css`
2. Add dark mode variants if needed
3. Update this documentation
4. Test across all themes

### Adding New Components
1. Use existing design tokens
2. Follow established patterns
3. Ensure responsive behavior
4. Test on all breakpoints

### Modifying Design Tokens
1. Update CSS variables in `base.css`
2. Test impact across all components
3. Update dark mode variants
4. Update this documentation

## Testing Checklist

### Visual Testing
- [ ] Colors render correctly in light mode
- [ ] Colors render correctly in dark mode
- [ ] Responsive breakpoints work correctly
- [ ] Text remains readable at all sizes
- [ ] Icons display properly
- [ ] Hover states work as expected
- [ ] No double shadows on chart components (cards should provide single shadow)
- [ ] Chart content stays within card boundaries (no overflow)

### Functional Testing
- [ ] Navigation works on all screen sizes
- [ ] Forms are usable on mobile devices
- [ ] Charts display correctly on different screens
- [ ] Theme switching works properly

### Accessibility Testing
- [ ] Color contrast ratios meet WCAG standards
- [ ] Focus indicators are visible
- [ ] Touch targets are appropriately sized
- [ ] Screen reader compatibility

---

**Last Updated**: November 30, 2025
**Version**: 1.2
**Maintained by**: Development Team</content>
<parameter name="filePath">/Users/macbook1/work/JackLimited/frontend/FRONTEND_STYLE_GUIDE.md