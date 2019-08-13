import React, { Component } from 'react';

export class TweetList extends Component {
    constructor(props) {
        super(props);
        this.state = {
            loading: true, 
            tweets: [],
            page: 0,
            pageSize: 100,
            autoUpdate: true
        }

        this.goForward = this.goForward.bind(this);
        this.goBackwards = this.goBackwards.bind(this);
        this.toggleRefresh = this.toggleRefresh.bind(this);
    }

    componentDidMount() {
        this.update();
        setInterval(() => this.state.autoUpdate && this.update(), 5000);
    }

    update () {
        let url = "/api/v1/tweets?page=" +
                    this.state.page +
                    "&pageSize=" +
                    this.state.pageSize;

        fetch(url)
            .then(res => res.json())
            .then(data => {
                this.setState({ loading: false, tweets: data });
            });
    }

    goForward (e) {
        e.preventDefault();
        this.setState({page: this.state.page + 1, loading: true});
        this.update();
    }

    goBackwards (e) {
        e.preventDefault();
        this.setState({page: this.state.page - 1, loading: true});
        this.update();
    }

    toggleRefresh (e) {
        e.preventDefault();
        this.setState({autoUpdate: !this.state.autoUpdate});
    }

    render () {
        if (this.state.loading)
        {
            return <p>Loading...</p>;
        } else {
            return <div>
                <table className='table table-striped'>
                    <thead>
                        <tr>
                            <th>Time</th>
                            <th>Tweet</th>
                            <th>User</th>
                            <th>Keywords</th>
                            <th>Location</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        {this.state.tweets.map(t => 
                            <tr key={t.id}>
                                <td>{t.createdAt}</td>
                                <td>{t.text}</td>
                                <td>{t.userHandle}</td>
                                <td>{t.matchedKeywords.join(", ")}</td>
                                <td>{t.location}</td>
                                <td>
                                    <a href={"https://twitter.com/"+t.userHandle+"/status/"+t.id}>
                                        <i className="fab fa-twitter"/>
                                    </a>
                                </td>
                            </tr>
                        )}
                    </tbody>
                </table>
                <div className="mx-auto">
                    <div>
                        { this.state.page !== 0 && <button className="btn btn-link" onClick={this.goBackwards} >« </button> }
                        {this.state.page}
                        <button className="btn btn-link" onClick={this.goForward} > »</button>
                    </div>
                    <div>
                        <button className="btn btn-link" onClick={this.toggleRefresh}>toggle auto updates</button>
                    </div>
                </div>
            </div>
        }
    }
}
