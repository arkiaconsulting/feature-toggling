import * as React from 'react';

interface IProps {
    name?: string;
}

const Header: React.FC<IProps> = (props: IProps) => (
    <h1>{props.name} with Azure App Configuration.</h1>
);

Header.defaultProps = {
    name: 'Feature Toggling',
};

export default Header;