import { forwardRef } from 'react'
import {
  Card,
  CardProps,
  CardContent,
  CardActions,
  CardHeader,
  Box,
  Skeleton
} from '@mui/material'
import { alpha } from '@mui/material/styles'

interface ModernCardProps extends Omit<CardProps, 'title' | 'variant'> {
  title?: React.ReactNode
  subtitle?: React.ReactNode
  actions?: React.ReactNode
  loading?: boolean
  hover?: boolean
  gradient?: boolean
  variant?: 'outlined' | 'elevated' | 'filled'
}

export const ModernCard = forwardRef<HTMLDivElement, ModernCardProps>(
  ({
    title,
    subtitle,
    actions,
    children,
    loading = false,
    hover = true,
    gradient = false,
    variant = 'elevated',
    sx,
    ...props
  }, ref) => {
    const getCardStyles = () => {
      const baseStyles = {
        position: 'relative',
        overflow: 'hidden',
        borderRadius: 2,
        transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
        ...(hover && {
          '&:hover': {
            transform: 'translateY(-2px)',
            boxShadow: (theme: any) => theme.shadows[8],
          }
        })
      }

      switch (variant) {
        case 'outlined':
          return {
            ...baseStyles,
            border: '1px solid',
            borderColor: 'divider',
            boxShadow: 'none',
            '&:hover': hover ? {
              ...baseStyles['&:hover'],
              borderColor: 'primary.main',
              boxShadow: (theme: any) => `0 0 0 1px ${alpha(theme.palette.primary.main, 0.2)}`,
            } : {}
          }

        case 'filled':
          return {
            ...baseStyles,
            backgroundColor: (theme: any) => alpha(theme.palette.primary.main, 0.04),
            border: 'none'
          }

        default: // elevated
          return {
            ...baseStyles,
            ...(gradient && {
              background: (theme: any) =>
                `linear-gradient(135deg, ${theme.palette.background.paper} 0%, ${alpha(theme.palette.primary.main, 0.02)} 100%)`
            })
          }
      }
    }

    if (loading) {
      return (
        <Card ref={ref} sx={{ ...getCardStyles(), ...(sx as any) }} {...props}>
          {title && (
            <CardHeader
              title={<Skeleton variant="text" width="60%" />}
              subheader={subtitle && <Skeleton variant="text" width="40%" />}
            />
          )}
          <CardContent>
            <Box sx={{ pt: 0.5 }}>
              <Skeleton variant="rectangular" width="100%" height={60} />
              <Skeleton variant="text" />
              <Skeleton variant="text" width="80%" />
            </Box>
          </CardContent>
          {actions && (
            <CardActions>
              <Skeleton variant="rectangular" width={80} height={36} />
              <Skeleton variant="rectangular" width={80} height={36} />
            </CardActions>
          )}
        </Card>
      )
    }

    return (
      <Card ref={ref} sx={{ ...getCardStyles(), ...(sx as any) }} {...props}>
        {title && (
          <CardHeader
            title={title}
            subheader={subtitle}
            titleTypographyProps={{
              variant: 'h6',
              component: 'h2'
            }}
          />
        )}

        <CardContent sx={{ pt: title ? 0 : 2 }}>
          {children}
        </CardContent>

        {actions && (
          <CardActions sx={{ px: 2, pb: 2 }}>
            {actions}
          </CardActions>
        )}
      </Card>
    )
  }
)