import React, { Component } from 'react';

import { TweetList } from './TweetList';
import { FollowedKeywords } from './FollowedKeywords';

export class Home extends Component {
  static displayName = Home.name;

  render () {
    return (
      <div>
        <h1>VOST Twitter Watcher</h1>
        < FollowedKeywords />
        <TweetList />
      </div>
    );
  }
}
