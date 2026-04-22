import React from 'react';

class ErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false, error: null, errorInfo: null };
  }

  static getDerivedStateFromError(error) {
    return { hasError: true };
  }

  componentDidCatch(error, errorInfo) {
    this.setState({
      error: error,
      errorInfo: errorInfo
    });
    console.error("ErrorBoundary caught an error", error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4">
          <div className="bg-slate-900 border border-slate-800 rounded-3xl p-8 max-w-lg w-full text-center shadow-2xl">
            <div className="text-6xl mb-6">💥</div>
            <h1 className="text-2xl font-black text-white mb-2 uppercase tracking-tight">
              Sistem Çöktü
            </h1>
            <p className="text-slate-400 mb-8">
              Beklenmedik bir hata oluştu. Mühendislerimiz durumu inceliyor (umarız).
            </p>
            <button
              onClick={() => window.location.reload()}
              className="bg-indigo-600 hover:bg-indigo-500 text-white px-8 py-3 rounded-xl font-bold transition-all shadow-lg shadow-indigo-500/25"
            >
              Yeniden Yükle
            </button>
            
            {process.env.NODE_ENV === 'development' && (
              <div className="mt-8 text-left bg-slate-950 p-4 rounded-xl overflow-auto text-xs text-rose-400 border border-rose-500/20 max-h-48">
                <p className="font-bold mb-1">{this.state.error?.toString()}</p>
                <p className="font-mono">{this.state.errorInfo?.componentStack}</p>
              </div>
            )}
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
