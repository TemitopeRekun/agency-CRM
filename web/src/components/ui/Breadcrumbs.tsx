'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { ChevronRight, Home } from 'lucide-react';

export function Breadcrumbs() {
  const pathname = usePathname();
  const paths = pathname.split('/').filter(Boolean);

  if (paths.length === 0 || pathname === '/dashboard') return null;

  return (
    <nav className="flex items-center space-x-2 text-xs font-medium text-slate-500 mb-6 bg-slate-50 w-fit px-3 py-1.5 rounded-full border shadow-sm">
      <Link href="/dashboard" className="flex items-center hover:text-indigo-600 transition-colors">
        <Home className="w-3 h-3 mr-1" />
        Home
      </Link>
      
      {paths.map((path, index) => {
        const href = `/${paths.slice(0, index + 1).join('/')}`;
        const isLast = index === paths.length - 1;
        const label = path.charAt(0).toUpperCase() + path.slice(1);

        return (
          <div key={path} className="flex items-center space-x-2">
            <ChevronRight className="w-3 h-3 text-slate-400" />
            {isLast ? (
              <span className="text-slate-900 font-bold">{label}</span>
            ) : (
              <Link href={href} className="hover:text-indigo-600 transition-colors">
                {label}
              </Link>
            )}
          </div>
        );
      })}
    </nav>
  );
}
