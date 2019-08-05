import React, { Component } from 'react';

export class TweetList extends Component {
    constructor(props) {
        super(props);
        this.state = {loading: true, tweets: []}
    }

    componentDidMount() {
        fetch("/api/v1/tweets")
            .then(res => res.json())
            .then(data => {
                this.setState({ loading: false, tweets: data });
            });
    }

    render () {
        if (this.state.loading)
        {
            return <p>Loading...</p>;
        } else {
            return <table className='table table-striped'>
                    <thead>
                        <tr>
                            <th>Time</th>
                            <th>Tweet</th>
                            <th>User</th>
                            <th>Loc.</th>
                        </tr>
                    </thead>
                    <tbody>
                        {this.state.tweets.map(t => 
                            <tr id={t.id}>
                                <td>{t.status.createdAt}</td>
                                <td>{t.status.text}</td>
                                <td>{t.status.user.name}</td>
                                <td>{ t.status.place !== null && t.status.place.name !== null
                                        ? t.status.place.name : "" }</td>
                            </tr>
                        )}
                    </tbody>
                </table>
        }
    }
}
