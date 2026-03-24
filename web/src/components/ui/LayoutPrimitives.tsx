import { ReactNode } from 'react';

export const Card = ({ children, className = '' }: { children: ReactNode; className?: string }) => (
  <div className={`rounded-xl border border-border bg-card text-card-foreground shadow-sm ${className}`}>
    {children}
  </div>
);

export const CardHeader = ({ children, className = '' }: { children: ReactNode; className?: string }) => (
  <div className={`flex flex-col space-y-1.5 p-6 ${className}`}>{children}</div>
);

export const CardTitle = ({ children, className = '' }: { children: ReactNode; className?: string }) => (
  <h3 className={`text-2xl font-semibold leading-none tracking-tight ${className}`}>{children}</h3>
);

export const CardContent = ({ children, className = '' }: { children: ReactNode; className?: string }) => (
  <div className={`p-6 pt-0 ${className}`}>{children}</div>
);

export const Container = ({ children, className = '' }: { children: ReactNode; className?: string }) => (
  <div className={`container mx-auto px-4 md:px-6 lg:px-8 ${className}`}>{children}</div>
);

export const Section = ({ children, title, className = '' }: { children: ReactNode; title?: string; className?: string }) => (
  <section className={`py-8 md:py-12 ${className}`}>
    {title && <h2 className="mb-6 text-3xl font-bold tracking-tight">{title}</h2>}
    {children}
  </section>
);
