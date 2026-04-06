'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';

const NAV_ITEMS = [
  { label: 'Clients', href: '/clients', color: 'hover:text-blue-400' },
  { label: 'Leads', href: '/leads', color: 'hover:text-green-400' },
  { label: 'Offers', href: '/offers', color: 'hover:text-purple-400' },
  { label: 'Projects', href: '/projects', color: 'hover:text-amber-400' },
  { label: 'Tasks', href: '/tasks', color: 'hover:text-emerald-400' },
  { label: 'Contracts', href: '/contracts', color: 'hover:text-pink-400' },
  { label: 'Invoices', href: '/invoices', color: 'hover:text-rose-400' },
  { label: 'Settings', href: '/settings', color: 'hover:text-slate-400' },
];

export default function Navbar() {
  const pathname = usePathname();

  return (
    <nav className="sticky top-0 z-50 w-full border-b bg-slate-950/80 backdrop-blur-md text-white shadow-lg">
      <div className="container mx-auto flex h-16 items-center justify-between px-4">
        <div className="flex items-center gap-8">
            <Link href="/dashboard" className="flex items-center gap-2">
                <div className="w-8 h-8 bg-gradient-to-br from-indigo-500 to-purple-600 rounded-lg flex items-center justify-center font-bold text-lg shadow-inner">A</div>
                <span className="text-xl font-bold tracking-tight bg-clip-text text-transparent bg-gradient-to-r from-white to-slate-400">
                    Agency CRM
                </span>
            </Link>
            
            <div className="hidden lg:flex items-center gap-1">
                {NAV_ITEMS.map((item) => (
                    <Link 
                        key={item.href}
                        href={item.href} 
                        className={`px-4 py-2 text-sm font-medium transition-all duration-200 rounded-md
                            ${pathname === item.href 
                                ? 'bg-white/10 text-white shadow-sm ring-1 ring-white/20' 
                                : `text-slate-400 ${item.color} hover:bg-white/5`
                            }`}
                    >
                        {item.label}
                    </Link>
                ))}
            </div>
        </div>

        <div className="flex items-center gap-4">
          <Link 
            href="/login" 
            className="text-sm font-semibold bg-rose-600/90 hover:bg-rose-600 px-4 py-2 rounded-lg transition-all shadow-md active:scale-95"
          >
            Logout
          </Link>
        </div>
      </div>
    </nav>
  );
}
