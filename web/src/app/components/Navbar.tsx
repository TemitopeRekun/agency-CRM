import Link from 'next/link';

export default function Navbar() {
  return (
    <nav className="bg-slate-900 text-white p-4 shadow-md">
      <div className="container mx-auto flex justify-between items-center">
        <Link href="/dashboard" className="text-xl font-bold">Agency CRM</Link>
        <div className="space-x-6">
          <Link href="/clients" className="hover:text-blue-400 transition">Clients</Link>
          <Link href="/leads" className="hover:text-green-400 transition">Leads</Link>
          <Link href="/offers" className="hover:text-purple-400 transition">Offers</Link>
          <Link href="/projects" className="hover:text-amber-400 transition">Projects</Link>
          <Link href="/tasks" className="hover:text-emerald-400 transition">Tasks</Link>
          <Link href="/contracts" className="hover:text-pink-400 transition">Contracts</Link>
          <Link href="/invoices" className="hover:text-rose-400 transition">Invoices</Link>
          <Link href="/analytics" className="hover:text-rose-400 transition">Analytics</Link>
          <Link href="/integrations" className="hover:text-cyan-400 font-bold transition">Integrations</Link>
          <Link href="/login" className="bg-red-600 px-3 py-1 rounded hover:bg-red-700 transition">Logout</Link>
        </div>
      </div>
    </nav>
  );
}
