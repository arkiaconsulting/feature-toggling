import React from 'react';
import logo from './logo.svg';
import './App.css';
import Header from './Header';
import Weather from './Weather'

const App: React.FC = () => {
  return (
    <div className="App">
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <Header name="Feature Toggling" />
      </header>
      <Weather defaultLocation={Weather.defaultProps.defaultLocation} />
    </div>
  );
}

export default App;
